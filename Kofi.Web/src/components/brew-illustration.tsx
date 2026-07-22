import { cn } from '@/lib/utils'

// A V60 on its server, drawn with the theme's colours so it reads in light and
// dark. The one moving part is the drip; it stays still for reduced motion.
export function BrewIllustration({ className }: { className?: string }) {
  return (
    <svg
      viewBox="0 0 400 320"
      role="img"
      aria-label="A V60 dripper brewing into a glass server, two coffee beans beside it"
      className={cn('text-foreground', className)}
      fill="none"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <circle cx="210" cy="176" r="138" className="fill-accent" />

      {/* Beans */}
      <g className="fill-coffee stroke-foreground" strokeWidth="2">
        <ellipse cx="84" cy="282" rx="15" ry="10" transform="rotate(-28 84 282)" />
        <ellipse cx="112" cy="296" rx="13" ry="9" transform="rotate(18 112 296)" />
      </g>
      <g className="stroke-accent" strokeWidth="2">
        <path d="M74 289 C82 283 86 280 94 275" />
        <path d="M103 293 C110 295 114 297 121 300" />
      </g>

      {/* Server */}
      <path d="M262 222 C294 222 294 270 260 270" className="stroke-foreground" strokeWidth="3" />
      <clipPath id="brew-server">
        <path d="M152 200 H248 V208 C262 218 268 240 264 264 C260 288 240 298 200 298 C160 298 140 288 136 264 C132 240 138 218 152 208 Z" />
      </clipPath>
      <g clipPath="url(#brew-server)">
        <rect x="120" y="200" width="160" height="110" className="fill-card" />
        <path d="M120 248 C150 242 170 254 200 248 C230 242 250 254 280 248 V310 H120 Z" className="fill-coffee" />
      </g>
      <path
        d="M152 200 H248 V208 C262 218 268 240 264 264 C260 288 240 298 200 298 C160 298 140 288 136 264 C132 240 138 218 152 208 Z"
        className="stroke-foreground"
        strokeWidth="2.5"
      />
      <path d="M150 226 C146 238 146 252 150 266" className="stroke-background" strokeWidth="3" opacity="0.7" />

      {/* The drip */}
      <path d="M200 192 C203 198 205 201 205 204 A5 5 0 0 1 195 204 C195 201 197 198 200 192 Z" className="brew-drip fill-coffee" />

      {/* Dripper */}
      <rect x="164" y="176" width="72" height="9" rx="4.5" className="fill-card stroke-foreground" strokeWidth="2.5" />
      {/* The paper filter is paper in either theme. */}
      <path d="M138 64 L150 92 H250 L262 64 Z" className="fill-[oklch(0.97_0.008_90)] stroke-foreground" strokeWidth="2" />
      <path d="M126 98 H274 L224 176 H176 Z" className="fill-card stroke-foreground" strokeWidth="2.5" />
      <g className="stroke-foreground" strokeWidth="1.5" opacity="0.35">
        <path d="M150 106 C164 128 174 148 184 168" />
        <path d="M176 106 C186 128 192 148 196 168" />
        <path d="M204 106 C208 128 208 148 206 168" />
        <path d="M232 106 C228 128 222 148 216 168" />
      </g>
      <rect x="116" y="88" width="168" height="12" rx="6" className="fill-card stroke-foreground" strokeWidth="2.5" />
      <path d="M284 102 C318 104 316 150 262 150" className="stroke-foreground" strokeWidth="3" />

      {/* Steam */}
      <g className="stroke-muted-foreground" strokeWidth="2" opacity="0.6">
        <path d="M186 48 C178 40 194 34 186 24" />
        <path d="M212 52 C204 44 220 38 212 28" />
      </g>
    </svg>
  )
}
