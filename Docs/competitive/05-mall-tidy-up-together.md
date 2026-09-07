# Mall: Tidy Up Together

- **Steam:** [store.steampowered.com/app/4962010](https://store.steampowered.com/app/4962010/Mall_Tidy_Up_Together/) (App ID 4962010)
- **Released:** August 21, 2026
- **Price:** $4.99 (10% launch discount at release)
- **Reviews (2026-09-06):** Mostly Positive — 228 total, 70.6% positive (161 pos / 67 neg)

## Premise & scope

Shelve 3,682 products into the correct stores across a 12-store mall (supermarket, bags,
bookstore, cosmetics, video games, gift shop, hardware, hats, toys, sporting goods, tobacco,
sunglasses). Story hook: a rival mall sabotaged this one before its grand opening. No deadline.

## Mechanics

- Pure visual sort across many small stores rather than one big space — a "mall" structuring
  device more than a mechanical difference from its peers.
- Single player, online co-op, **and online PvP** — the only title in this set offering
  competitive (not just cooperative) multiplayer.
- Sandbox Mode (all skills unlocked) and No-Skill Mode (challenge) bookend the normal progression.

## Update history

Only one public news post found as of 2026-09-06: the "OUT NOW" launch announcement
(2026-08-21). No visible patch notes since — either the game hasn't needed hotfixes, or updates
aren't being posted to the Steam news feed. Worth spot-checking again given the review score.

## Common complaints (negative Steam reviews, sampled 2026-09-07) — this answers "why Mostly Positive"

- **The most intense and consistent "AI slop" backlash in this entire competitive set.** Nearly
  every sampled negative review leads with it, several in all-caps: "undisclosed ai assests and
  textures. looks awful," "MASSIVE amounts of AI... extremely obvious especially on the
  book/magazine covers," "Every product cover has janky AI writing on it." This alone appears to
  be the dominant driver of the weak score.
- **A direct, named echo of BookBoy's own already-fixed occlusion bug**: "Larger items are held
  in front of you, blocking the crosshair completely and the surrounding view as well... you
  can't pick up items at all." Multiple independent reviewers report this. BookBoy hit and fixed
  this exact problem (held-book scale reduced, carry-point repositioned) after direct playtest
  feedback — this is live confirmation it's a real, recurring failure mode in the genre, not a
  one-off.
- **Frequent completion-blocking bugs**: items clipping into the floor/seats and becoming
  permanently unreachable without ability-assists, several reports of literally being unable to
  finish the game because a required item is missing or a shelf incorrectly reads as complete
  already.
- Direct accusations of copying Librarian's mechanics/assets without adaptation ("skills in the
  game do not function... they were ripped from Librarian"), and criticism of the developer
  shipping a near-identical second game (`Megastore`, also in this set) one week later instead of
  patching this one.

## Relevance to BookBoy

- Now that the negative reviews are in hand, the weak score isn't ambiguous: it's dominated by
  (a) AI-disclosure backlash and (b) exactly the item-blocks-your-view bug BookBoy already caught
  and fixed via direct playtesting. Strong retroactive validation that fixing that bug
  proactively/quickly was the right call, not a nice-to-have.
- The PvP mode is a useful negative case for BookBoy's "cozy, no-fail play" principle
  ([01-vision-and-design.md](../01-vision-and-design.md)) — competitive time pressure is the
  opposite of what BookBoy is going for, and this is the one title in the set actually trying it.
- Completion-blocking bugs (unreachable items, shelves reading complete when they aren't) are a
  concrete argument for keeping placement/completion logic simple and directly testable — exactly
  what `CleanupTracker`'s straightforward "every slot occupied" check (no complex state) already
  does.
