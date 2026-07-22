import { BrewIllustration } from '@/components/brew-illustration'
import { keepPreviousData } from '@tanstack/react-query'
import { createFileRoute, useNavigate, useRouter } from '@tanstack/react-router'
import { Search, X } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { z } from 'zod'
import { getGetProductsQueryOptions, useGetProducts } from '@/api/generated/endpoints'
import { ProductTile } from '@/components/product-tile'
import { ErrorState } from '@/components/states'
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from '@/components/ui/empty'
import { InputGroup, InputGroupAddon, InputGroupButton, InputGroupInput } from '@/components/ui/input-group'
import { Pager } from '@/components/pager'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'

const PAGE_SIZE = 9

const searchSchema = z.object({
  q: z.string().optional().catch(undefined),
  sort: z.enum(['name', 'price', 'price_desc']).optional().catch(undefined),
  page: z.number().int().min(1).optional().catch(undefined),
})

type CatalogSearch = z.infer<typeof searchSchema>
const catalogParams = ({ q, sort, page }: CatalogSearch) => ({
  search: q,
  sort: sort === 'name' ? undefined : sort,
  page: page ?? 1,
  pageSize: PAGE_SIZE,
})

export const Route = createFileRoute('/_shop/')({
  validateSearch: searchSchema,
  loaderDeps: ({ search }) => search,
  loader: ({ context, deps }) => context.queryClient.prefetchQuery(getGetProductsQueryOptions(catalogParams(deps))),
  component: Catalog,
})

function Catalog() {
  const { q, sort, page = 1 } = Route.useSearch()
  const navigate = useNavigate({ from: Route.fullPath })
  const router = useRouter()
  const [term, setTerm] = useState(q ?? '')
  // The q the search box last wrote to the URL. When the catalog's URL
  // changes to any other q (back, forward, a link), the box follows it.
  // This listens to the history itself: a back press can land before the
  // router has caught up with what the box wrote, so q alone may never
  // appear to change.
  const written = useRef(q)

  useEffect(
    () =>
      router.history.subscribe(({ location }) => {
        // Leaving the catalog is not a new search.
        if (location.pathname !== Route.fullPath) return
        const next = router.options.parseSearch(location.search).q
        const urlTerm = typeof next === 'string' ? next : undefined
        if (urlTerm !== written.current) {
          written.current = urlTerm
          setTerm(urlTerm ?? '')
        }
      }),
    [router],
  )

  // Search as you type, but only once typing pauses.
  useEffect(() => {
    const t = setTimeout(() => {
      // Typing just before following a link must not pull the page back.
      if (router.history.location.pathname !== Route.fullPath) return
      if ((term || undefined) !== q) {
        written.current = term || undefined
        navigate({ search: (s) => ({ ...s, q: term || undefined, page: undefined }), replace: true, resetScroll: false })
      }
    }, 300)
    return () => clearTimeout(t)
  }, [term, q, navigate, router])

  const products = useGetProducts(catalogParams({ q, sort, page }), { query: { placeholderData: keepPreviousData } })
  const total = Number(products.data?.totalCount ?? 0)

  return (
    <div className="space-y-10">
      <section className="grid items-center gap-8 border-b pb-10 md:grid-cols-[1.4fr_1fr] lg:gap-16">
        <div className="space-y-4">
          <h1 className="text-4xl font-bold sm:text-5xl">Everything for a better cup at home.</h1>
          <p className="max-w-prose text-lg text-muted-foreground">
            Freshly roasted beans and capsules from a dozen Czech roasters, and the grinders, drippers and kettles
            to brew them.
          </p>
        </div>
        <BrewIllustration className="mx-auto w-full max-w-md" />
      </section>

      <section className="space-y-6">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <InputGroup className="sm:max-w-sm">
            <InputGroupAddon>
              <Search />
            </InputGroupAddon>
            <InputGroupInput
              placeholder="Search coffee, origin, kit"
              aria-label="Search the shop"
              value={term}
              onChange={(e) => setTerm(e.target.value)}
            />
            {term && (
              <InputGroupAddon align="inline-end">
                <InputGroupButton size="icon-xs" aria-label="Clear search" onClick={() => setTerm('')}>
                  <X />
                </InputGroupButton>
              </InputGroupAddon>
            )}
          </InputGroup>
          <Select
            value={sort ?? 'name'}
            onValueChange={(value) =>
              navigate({
                search: (s) => ({ ...s, sort: value === 'name' ? undefined : (value as 'price'), page: undefined }),
                // The toolbar stays where it is while the results change.
                resetScroll: false,
              })
            }
          >
            <SelectTrigger className="sm:w-48" aria-label="Sort by">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="name">Name</SelectItem>
              <SelectItem value="price">Price, low to high</SelectItem>
              <SelectItem value="price_desc">Price, high to low</SelectItem>
            </SelectContent>
          </Select>
          {products.data && (
            <p className="text-sm text-muted-foreground sm:ml-auto">
              {total} {total === 1 ? 'product' : 'products'}
            </p>
          )}
        </div>

        {products.isError ? (
          <ErrorState error={products.error} onRetry={() => products.refetch()} />
        ) : products.isPending ? (
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }, (_, i) => (
              <Skeleton key={i} className="h-80 rounded-xl" />
            ))}
          </div>
        ) : products.data.items?.length ? (
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {products.data.items.map((p) => (
              <ProductTile key={p.id} product={p} />
            ))}
          </div>
        ) : (
          <Empty className="border">
            <EmptyHeader>
              <EmptyTitle>Nothing matches “{q}”</EmptyTitle>
              <EmptyDescription>Try an origin like Etiopie, or a word like mlýnek.</EmptyDescription>
            </EmptyHeader>
          </Empty>
        )}

        <Pager
          page={page}
          pageCount={Math.ceil(total / PAGE_SIZE)}
          onPage={(p) => navigate({ search: (s) => ({ ...s, page: p === 1 ? undefined : p }) })}
        />
      </section>
    </div>
  )
}
