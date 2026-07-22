const money = new Intl.NumberFormat('en-IE', { style: 'currency', currency: 'EUR' })
const date = new Intl.DateTimeFormat('en-GB', { day: 'numeric', month: 'short', year: 'numeric' })
const dateTime = new Intl.DateTimeFormat('en-GB', {
  day: 'numeric',
  month: 'short',
  hour: '2-digit',
  minute: '2-digit',
})

export const formatMoney = (value: number | string) => money.format(Number(value))
export const formatDate = (value: string) => date.format(new Date(value))
export const formatDateTime = (value: string) => dateTime.format(new Date(value))

export function daysSince(isoDate: string) {
  const then = new Date(`${isoDate}T00:00:00`)
  const now = new Date()
  now.setHours(0, 0, 0, 0)
  return Math.round((now.getTime() - then.getTime()) / 86_400_000)
}
