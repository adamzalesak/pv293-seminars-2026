import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from '@/components/ui/pagination'

export function Pager({ page, pageCount, onPage }: { page: number; pageCount: number; onPage: (page: number) => void }) {
  if (pageCount <= 1) return null
  const go = (p: number) => (e: React.MouseEvent) => {
    e.preventDefault()
    if (p >= 1 && p <= pageCount && p !== page) onPage(p)
  }
  return (
    <Pagination>
      <PaginationContent>
        <PaginationItem>
          <PaginationPrevious href="#" onClick={go(page - 1)} aria-disabled={page === 1} className="aria-disabled:opacity-40" />
        </PaginationItem>
        {Array.from({ length: pageCount }, (_, i) => i + 1).map((p) => (
          <PaginationItem key={p}>
            <PaginationLink href="#" isActive={p === page} onClick={go(p)}>
              {p}
            </PaginationLink>
          </PaginationItem>
        ))}
        <PaginationItem>
          <PaginationNext href="#" onClick={go(page + 1)} aria-disabled={page === pageCount} className="aria-disabled:opacity-40" />
        </PaginationItem>
      </PaginationContent>
    </Pagination>
  )
}
