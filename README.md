# KoFI

A coffee e-shop we are building this semester: coffee from a dozen
roasteries and the gear to brew it — beans by origin, capsules, grinders,
kettles. It needs a catalogue customers can browse and search,
orders they can place, pay for and cancel, stock that gets reserved when
an order is placed, payments through an external gateway, and e-mails
confirming orders and shipping.

What we are up against: **the first paying customer this quarter, one
team of a handful of developers, tens to hundreds of orders a day in
Czechia — and, if it works, the rest of Europe.**

There is no code yet. Today is design day — we decide what this system is
going to look like, and we write those decisions down.

## What lives here

```
docs/adr/            architecture decision records, one file per decision
docs/architecture/   the C4 model of the system (LikeC4)
```

Both grow through the semester. By the end, `git log` over these two folders
tells the whole story of how the system got the way it is.

## The C4 model

```
npx likec4 start                     # interactive preview in the browser
npx likec4 export png -o docs/architecture/
```
