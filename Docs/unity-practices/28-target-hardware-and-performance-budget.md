# 28. Target hardware profile and frame-time budget for a 4070 Super dev machine

Research date 2026-10-19 (every "read" date below is this date unless stated). Project: Unity 6000.6.6f1, URP, Windows x64, headed for Steam. Nothing was run in Unity; no project files were changed.
Tags: **[fetched]** = I read the page this session; **[search-only]** = from a search summary, page not read; **[derived]** = my arithmetic on fetched numbers; **[local]** = this repo; **UNSOURCED GUESS** = no source.
Builds on file 10 (batching, draw-call rules of thumb) and file 24 (player builds, `FrameTimingManager`, proposed p95 <= 8.3 ms). This file replaces the "2x slower GPU" guess in file 24 section 2.3 with sourced ratios.

## 1. Question

The developer's PC has an RTX 4070 Super (12 GB), far above the typical Steam player, and he wants to test up to 1000 books in view. What is a sensible target hardware profile (minimum and recommended), how fast is the 4070 Super relative to it and to the median Steam GPU, and therefore how many milliseconds of GPU time and CPU time per frame may BookBoy use when measured on the 4070 Super at 1080p so the minimum hardware still holds 60 fps (and 30 fps as a floor)? How can the 4070 Super emulate a weaker GPU, and what do Steam Deck and laptops change?

## 2. Findings

### 2a. Steam Hardware & Software Survey, September 2026 (the current month on the page)

- Survey month is **September 2026** on every page below. Windows 95.03%; Windows 11 71.34%, Windows 10 23.61%; Linux 3.05% (SteamOS Holo 0.53%). [fetched 2026-10-19] https://store.steampowered.com/hwsurvey
- **Top GPUs** (All Video Cards table, share / change): RTX 5070 6.15%, RTX 5060 4.40%, RTX 5060 Ti 4.06%, RTX 4060 3.89%, RTX 3060 3.66%, RTX 4060 Laptop 3.50%, RTX 3050 2.90%, RTX 4060 Ti 2.85%, RTX 5060 Laptop 2.45%, **GTX 1650 2.19%**, RTX 4070 2.11%, RTX 3060 Ti 2.08%, RTX 5080 1.98%, RTX 3070 1.97%, RTX 5070 Ti 1.87%, **RTX 4070 SUPER 1.74%** (rank 16), RTX 3060 Laptop 1.66%, RTX 4050 Laptop 1.59%, RTX 2060 1.53%, Intel Iris Xe 1.52%, AMD Radeon Graphics 1.51%, RX 9070 XT 1.42%, **GTX 1060 1.34%**, RTX 3080 1.30%, GTX 1660 SUPER 1.26%, Intel UHD Graphics 1.06%, **GTX 1050 Ti 1.03%**. 97 rows; "Other" 8.33%. [fetched] https://store.steampowered.com/hwsurvey/videocard/
- **Source disagreement:** the landing page's short top-8 list shows the same cards slightly lower (RTX 5070 5.86%, RTX 3060 3.54%, GTX 1650 2.10%) than the dedicated video-card page (6.15%, 3.66%, 2.19%). The pages do not say why (likely a different filter or snapshot). I use the dedicated page. [fetched] https://store.steampowered.com/hwsurvey and https://store.steampowered.com/hwsurvey/videocard/
- **VRAM** (Video Card): 16 GB 27.21%, 8 GB 26.71%, 12 GB 13.06%, 24 GB 5.50%, 4 GB 5.13%, 6 GB 5.12%, 2 GB 3.69%, 512 MB 2.60%, 10 GB 1.91%, 1 GB 1.44%, 20 GB 1.39%, 3 GB 1.20%, others small. Odd tail (64 GB 0.50%, 32 GB 1.41%) suggests the field includes shared/unified memory; the page does not say. [fetched] https://store.steampowered.com/hwsurvey
- **Primary display resolution:** 1920x1080 47.91% (-2.61), 2560x1440 27.04% (**+5.18** in one month), 2560x1600 5.22%, 3840x2160 4.79%, 3440x1440 2.91%, 1920x1200 2.56%, 1366x768 1.92%, 1440x900 0.62%, 2560x1080 0.61%, 1600x900 0.58%. [fetched] https://store.steampowered.com/hwsurvey
- **CPU cores (physical):** 8 cores 30.06%, 6 cores 27.23%, 4 cores 10.79%, 10 cores 8.19%, 16 cores 5.78%, 14 cores 5.23%, 12 cores 4.78%, 24 cores 2.78%, 2 cores 2.45%, 20 cores 2.09%. Vendor (Windows): Intel 52.59%, AMD 47.41%. [fetched] https://store.steampowered.com/hwsurvey and https://store.steampowered.com/hwsurvey/processormfg/
- **System RAM:** 32 GB 42.22%, 16 GB 37.82%, 8 GB 6.47%, 64 GB 3.71%, 24 GB 2.56%, 12 GB 2.12%, 48 GB 1.46%, 4 GB 1.15%. [fetched] https://store.steampowered.com/hwsurvey
- The survey is opt-in and sampled; the 1440p jump (+5.18 points in a month) looks like a sample shift. Older articles I saw in earlier research gave 1080p at 61-65% (see file 24, search-only); the Valve page is primary and newest.

### 2b. Published minimum/recommended specs of comparable games (Steam store pages, all read 2026-10-19)

Review figures are as shown on the page that day; all are 2026 releases so review counts are small for the newest.

| Game (release; reviews) | Min GPU | Min CPU / RAM | Rec GPU | Rec CPU / RAM |
|---|---|---|---|---|
| Librarian: Tidy Up the Arcane Library (Apr 30; 93% of 16,557) | GTX 980 Ti 6GB / RX 6500 XT | i7-4790K / Ryzen 5 1500X; 8 GB | GTX 1080 / RX 5700 XT | i7-8700 / Ryzen 5 3600; 16 GB |
| Supermarket Chaos (Jun 29; 90% of 1,563) | GTX 980 Ti 6GB / RX 6500 XT | same as Librarian; 8 GB | GTX 1080 / RX 5700 XT | same as Librarian; 16 GB |
| Shelves and Sorcery (Aug 11; 94% of 778) | any DX11/12 card with >= 4 GB VRAM | quad-core 2.5 GHz; page says "8 MB" RAM (typo) | none given | none given |
| Witch Cabin Cleanup (Oct 12; 98% of 248) | GTX 1050 / RX 580 | 2.5 GHz quad-core; 4 GB | none given | none given |
| Shelve the Potions (Aug 24; 87% of 607) | GTX 1050 / RX 560 | i3 / Ryzen 3; 4 GB | none given | none given |
| Wand & Order (Sep 23; 89% of 548) | GTX 1050 / AMD equivalent | i3 / FX; 8 GB | GTX 1060 | i3 / FX; 16 GB |
| Mall Tidy Up Together (Aug 21; 69% of 342, Mixed) | GTX 1060 / R9 290X | i5-11400F / Ryzen 5 5600X; 8 GB | GTX 1070 / RX 6600 | i5-13400F / Ryzen 5 7600X; 16 GB |
| Black Friday Janitor (Sep 9; 97% of 771) | GTX 1060 | quad-core; 8 GB | "GTX 2060" (sic) | not listed; 16 GB |
| Plushy Store Tidy Up (Sep 1; 51% of 56, Mixed) | GTX 750 Ti / R7 360 | i3-6100 / Ryzen 3 1200; 8 GB | GTX 1060 / RX 580 | i5-8400 / Ryzen 5 2600; 16 GB |

Sources [fetched]: https://store.steampowered.com/app/4197610/Librarian_Tidy_Up_the_Arcane_Library/ , https://store.steampowered.com/app/4800590/Supermarket_Chaos/ , https://store.steampowered.com/app/3614130/Shelves_and_Sorcery_Tidy_Up_the_Enchanted_Shop/ , https://store.steampowered.com/app/4991240/ , https://store.steampowered.com/app/4928820/Shelve_the_Potions , https://store.steampowered.com/app/4981570/Wand__Order_Tidy_up_the_magic_shop , https://store.steampowered.com/app/4962010/Mall_Tidy_Up_Together , https://store.steampowered.com/app/3396010/Black_Friday_Janitor_Clean_the_Whole_Mall , https://store.steampowered.com/app/1470810/Plushy_Store_Tidy_Up . Also read: Spellbound & Shelved (min "Intel HD 4000 / GeForce 400 / Radeon HD 5000", 2 GB RAM, dual-core; Very Positive 89% of 548 per the page) https://store.steampowered.com/app/5097840/Spellbound__Shelved ; it is clearly a very light game and not a comparison for 3D scenes.

- Librarian and Supermarket Chaos list **identical** CPU/GPU lines (same strings). Whether that is a shared template or shared publisher is not stated. UNSOURCED GUESS: the lines are template text rather than measured requirements.
- The "GTX 980 Ti / RX 6500 XT" pairing is not equivalent by PassMark (980 Ti 13,615 vs RX 6500 XT 9,615) [derived from section 2c], which also suggests loosely authored specs.
- Cohort summary [derived]: minimum GPU is GTX 1050 (3 games), GTX 1060 (2), GTX 750 Ti (1), GTX 980 Ti (2, one template), "4 GB VRAM DX11" (1). Recommended GPU, where given, is GTX 1060 to GTX 1080 / RX 5700 XT. Minimum RAM 4 to 8 GB, recommended 16 GB. Min CPU is 2.5 GHz quad-core to i7-4790K / Ryzen 5 1500X; the heaviest title in the group (Librarian, the one with about 3000 books per the task brief, not independently verified here) has the highest minimum (980 Ti).
- Best-reviewed (>= 90% and >= 700 reviews): Librarian, Black Friday Janitor, Shelves and Sorcery, Supermarket Chaos. Their minimums run GTX 980 Ti, GTX 1060, 4 GB card, GTX 980 Ti. [derived]

### 2c. Relative GPU performance (sources disagree on size, agree on order)

- **Tom's Hardware 2026 raster hierarchy, as normalized by TechFuelHQ** (11 games, Ultra, no upscaling, Ryzen 7 9800X3D; RTX 5090 = 100; page marked updated 2026-10-10; Tom's data dated 2026-06-24): 1080p: RTX 4070 SUPER **62.2**, RTX 5070 65.1, RTX 4070 54.7, RTX 5060 43.4, RTX 4060 **35.1**, RTX 5050 34.0, RTX 3060 12GB **30.2**. 1440p: 4070 SUPER 54.5, RTX 4060 28.4, RTX 3060 25.0. 4K: 44.4 / 15.7 / 20.0. RTX 3050 appears only as raw 44.6 fps at 1080p. GTX 1650/1060/1050/980 Ti are **not in the table** (older cards excluded). [fetched 2026-10-19] https://techfuelhq.com/articles/gpu-hierarchy-2026/ (a re-hosted secondary summary; I could not open Tom's own page, a fetch returned only the title).
- **PassMark G3D Mark (synthetic):** RTX 4070 SUPER 29,941; RTX 5070 28,670; RTX 4060 19,482; RTX 3060 16,885; RTX 3050 8GB 12,459; GTX 980 Ti 13,615; GTX 1660 SUPER 12,657; RTX 2060 14,073; GTX 1060 10,017; RX 6500 XT 9,615; RX 580 8,782; **GTX 1650 7,867**; RTX 2050 7,729; GTX 1050 Ti 6,368; GTX 1050 5,053; Radeon 780M 6,727; Intel Iris Xe 2,586; UHD 630 1,224. [fetched 2026-10-19] https://www.videocardbenchmark.net/high_end_gpus.html , https://www.videocardbenchmark.net/mid_range_gpus.html (page dates not shown).
- **technical.city pair pages** (aggregate score, and average 1080p fps over the games both cards were tested in): 4070 SUPER 71.57 vs GTX 1650 18.80 (**3.81x aggregate, 3.51x 1080p fps**); vs GTX 1060 6GB 24.50 (**2.92x, 2.33x fps**); vs GTX 980 Ti 32.54 (**2.20x, 2.14x fps**); vs RTX 4060 46.56 (**1.54x, 1.74x fps**). Its "RTX 3050" page actually showed the **mobile** RTX 3050 (4.33 TFLOPS) so I do not use it. [fetched 2026-10-19] https://technical.city/en/video/GeForce-GTX-1650-vs-GeForce-RTX-4070-SUPER , https://technical.city/en/video/GeForce-GTX-1060-vs-GeForce-RTX-4070-SUPER , https://technical.city/en/video/GeForce-GTX-980-Ti-vs-GeForce-RTX-4070-SUPER , https://technical.city/en/video/GeForce-RTX-4060-vs-GeForce-RTX-4070-SUPER , https://technical.city/en/video/GeForce-RTX-3050-vs-GeForce-RTX-4070-SUPER . The fps averages use different game sets per pair, so they are not comparable across pairs.
- **TechPowerUp** (the requested primary) returned HTTP 403 for its GPU database and review pages; not used. [fetched attempt, failed]
- **Raw specs** (FP32 TFLOPS / ROPs / bandwidth): RTX 4070 SUPER 35.48 / 80 / 504.2 GB/s, 7168 CUDA cores, 12 GB GDDR6X 192-bit, 220 W, boost 2.48 GHz; GTX 1650 (GDDR5) 2.984 / 32 / 128 GB/s; GTX 1060 6GB 4.375 / 48 / 192 GB/s; GTX 980 Ti 6.06 / 96 / 336.5 GB/s; RTX 4060 15.11 / 48 / 272 GB/s. [fetched] https://www.nvidia.com/en-us/geforce/graphics-cards/40-series/rtx-4070-family/ (cores, clock, memory, 220 W; page has placeholder price), https://technical.city/en/video/GeForce-GTX-1650-vs-GeForce-RTX-4070-SUPER (and the other pair pages above); Wikipedia GPU list for GTX 1650 2.984 TFLOPS / GTX 1060 4.372 / GTX 1050 1.862 / 980 Ti 6.060 [fetched] https://en.wikipedia.org/wiki/List_of_Nvidia_graphics_processing_units . Nominal TFLOPS (11.9x vs the 1650) overstate real game gain (3.5x); do not use TFLOPS as the ratio.
- **Where the 4070 SUPER sits in the survey** [derived from 2a + PassMark]: about 8% of survey share is strictly faster, so the card is around the top 10% of Steam GPUs; about 90% of players have an equal or slower GPU.
- **Median Steam GPU** [derived]: ordering the 97 listed rows by PassMark (using fetched values; my own rough estimates for about 25 rows I did not fetch, all clearly high-end or clearly low-end so they do not move the middle) the share-weighted median of the listed rows lands at about PassMark 17,000-19,500, i.e. an RTX 3060 / RTX 4060 (laptop) class card.

### 2d. Resolution scale, upscaling and Unity

- Unity: resolution scale renders below final size then upscales; "To reduce the amount of work for the GPU and speed up rendering, you can change the resolution of the image Unity renders." URP upscalers: Bilinear, Nearest-neighbor, STP (not with dynamic resolution), FSR 1; DLSS 4 SR, TAAU, FSR 2 are HDRP-only in the table. The page says nothing about values above 1. [fetched 2026-10-19] https://docs.unity.com/en-us/engine/6000.6/manual/cameras/resolution-scale/introduction
- Unity URP perf page: Render Scale "Set to below 1.0"; Upscaling Filter "Bilinear or Nearest-Neighbor"; lower shadow resolution, cascades, max distance, soft shadows off, additional-light shadows off; "Per Object Limit has no effect under Deferred or Forward+." [fetched] https://docs.unity3d.com/Manual/urp/optimize-for-better-performance.html
- `UniversalRenderPipelineAsset.renderScale`: "Specifies the render scale which scales the render target resolution used by this UniversalRenderPipelineAsset." No range stated. [fetched, URP 15.0 API page] https://docs.unity3d.com/kr/Packages/com.unity.render-pipelines.universal@15.0/api/UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset.html . **I could not find a Unity page stating the maximum.** UNSOURCED GUESS: the URP asset slider allows about 0.1 to 2.0 (memory of the asset inspector); verify in the Editor. A search result noted that in XR, changing render scale in the URP asset reallocates the eye texture and is expensive (do not do it every frame). [search-only] https://docs.unity3d.com/Manual/xr-graphics-resolution-scaling.html
- Player command line (relevant to the test harness): `-screen-width`, `-screen-height`, `-window-mode exclusive|borderless`, `-force-d3d11`, `-force-d3d12`, `-force-device-index` (choose a GPU); the page lists no CPU-core or job-worker limit flag. [fetched] https://docs.unity3d.com/6000.0/Documentation/Manual/PlayerCommandLineArguments.html . UNSOURCED GUESS: a `-job-worker-count` flag exists (I recall it) but I could not confirm it in the manual.

### 2e. Emulating a weaker GPU on the 4070 Super

- **Lock clocks (NVIDIA):** `nvidia-smi -q -d SUPPORTED_CLOCKS` to list valid clocks, `nvidia-smi -lgc <clock>` (lock core), `nvidia-smi -lmc <clock>` (lock memory), reset with `-rgc` / `-rmc`; run as administrator in the host Windows shell. [fetched] https://docs.nvidia.com/nim/wsl2/latest/benchmarking.html . Long forms `--lock-gpu-clocks`, `--lock-memory-clocks`, `--reset-gpu-clocks`, `--reset-memory-clocks`; NVIDIA says processors change clocks during execution, "introducing errors in measurements", so lock before measuring and reset after; Nsight Graphics GPU Trace has a "Lock Clocks to Base" checkbox. [fetched] https://developer.nvidia.com/blog/advanced-api-performance-setstablepowerstate . A third-party note says not all GPUs support lock options; check the supported-clocks list on the card. [search-only] https://docs.nvidia.com/nim/wsl2/1.0.0/benchmarking.html
- **Power limit:** `nvidia-smi -q -d POWER` shows min/max enforceable limits; users report `-pl` working on some GeForce cards on Windows (GTX 1060) and failing with "not supported" on others/Linux. [search-only] https://forum.level1techs.com/t/solved-how-msi-afterburner-does-it-in-windows/173075 , https://forums.developer.nvidia.com/t/the-default-power-limit-of-my-4060-laptop-halves-its-performance/294699 . I did not fetch MSI Afterburner documentation. UNSOURCED GUESS: Afterburner's power-limit slider and clock-curve editor can lower clocks on a 4070 Super, but the achievable floor is limited by the card's minimum power limit.
- **Quality levels:** each Unity Quality level can point to its own URP asset; the quality-presets page lists Render Scale 1 for both Low and High presets. [search-only] https://docs.unity3d.com/Manual/urp/birp-onboarding/quality-presets.html

### 2f. Steam Deck and laptops

- **Steam Deck Verified:** the game "must ship with a default configuration that results in a playable framerate", defined as **30 fps at 800p on Steam Deck** and 30 fps at 1080p on Steam Machine; smallest font >= 9 px tall at 1280x800, 12 px recommended. [fetched 2026-10-19] https://partner.steamgames.com/doc/steamdeck/compat
- **Deck hardware:** 6 nm AMD APU, 4-15 W APU power; CPU Zen 2, 4 cores / 8 threads, 2.4-3.5 GHz; GPU 8 RDNA 2 CUs at 1.6 GHz, **1.6 TFLOPS FP32**; 16 GB LPDDR5 shared; 1280x800, up to 90 Hz (OLED model shown). [fetched] https://www.steamdeck.com/en/tech . The survey lists SteamOS Holo at 0.53% of all systems, but the Deck GPU does not appear in the top 97 rows. [fetched] https://store.steampowered.com/hwsurvey and /hwsurvey/videocard/
- **Laptop dGPUs are a large block:** the named laptop NVIDIA GPUs in the top-97 list sum to about 13.8% of survey share (4060 Laptop 3.50, 5060 Laptop 2.45, 3060 Laptop 1.66, 4050 Laptop 1.59, 4070 Laptop 0.86, and others) [derived from 2a]. Notebookcheck: the laptop RTX 4060 TGP is configurable from 35 W to 115 W, boost clock about 1470 MHz at 35 W to 2370 MHz at 115 W, and low-power variants rank "clearly lower". [search-only] https://notebookcheck.nl/NVIDIA-NVIDIA-GeForce-RTX-4060.771830.0.html . So a laptop named "RTX 4060 Laptop" can be well below the desktop 4060 used in section 2c.
- **Integrated graphics in the survey** [derived from 2a]: Intel Iris Xe, UHD (several), HD, Arc Graphics, and AMD "Radeon Graphics" / 780M / Vega rows sum to about **8.3%** of share (the generic "AMD Radeon Graphics" rows, 1.51% + 0.82% + 0.42%, could include some non-integrated parts; the page does not say).

## 3. Numbers and rules of thumb

### 3a. Survey shares derived from the top-97 list (September 2026)

"Listed" = the 97 rows minus "Other" (91.41% of all systems). Thresholds use PassMark scores (fetched where possible; about 25 low-share rows used my rough estimates, shown in the working in the scratchpad only; error is about +/- 1 point on the lower buckets). [derived]

| Bucket | Share of all systems | Note |
|---|---|---|
| Integrated graphics (named iGPU rows) | ~8.3% | Iris Xe 1.52, UHD 1.06+0.61, AMD Radeon Graphics 1.51+0.82+0.42, others |
| Discrete or iGPU at or below GTX 1650 (PassMark <= ~8,000) | ~13.6% | includes the 8.3% iGPUs; discrete only ~5.3% (1650 2.19, 1050 Ti 1.03, 1050 0.47, RTX 2050 0.42, RX 580 2048SP 0.51, RX 570 0.31, GT 1030, RX 550) |
| At or below GTX 1060 6GB (PassMark <= ~10,100) | ~16.1% | |
| At or below RTX 3050 8GB (<= ~12,500) | ~21.6% | |
| At or below RTX 3060 (<= ~16,900) | ~39.0% | |
| Median (share-weighted) | ~17,000-19,500 | RTX 3060 / 4060 laptop class |
| Strictly faster than RTX 4070 SUPER | ~8.2% | so the dev card is roughly top 10% |
| VRAM >= 8 GB | ~79.6% | sum of 8, 10, 11, 12, 16, 20, 22, 24, 32, 64 GB rows |
| VRAM >= 6 GB / >= 4 GB | ~84.7% / ~89.8% | |
| Resolution 1080p-class (<= 1920x1200 incl. 1366x768, 1600x900, 2560x1080) | ~54.2% | |
| Resolution 1440p-class or higher (2560x1440/1600, 3440x1440, 3840x2160) | ~40.0% | 1440p alone 27.04% |
| Physical cores >= 6 / exactly 4 / <= 2 | ~86.1% (8+6+10+16+14+12+24+20) / 10.8% / 2.5% | |
| RAM >= 16 GB / >= 8 GB | ~89.2% (16+32+64+24+48+60+>64 rows; 12 GB adds 2.1) / ~97.8% (16 GB+ plus 12 GB and 8 GB rows) | |

### 3b. Ratios of the 4070 SUPER to candidate GPUs (higher = the 4070S is faster; slower GPU takes this many times longer per GPU-bound frame)

| Target GPU | Synthetic (PassMark) | Game-based | Range used |
|---|---|---|---|
| GTX 1650 | 3.81 | 3.81 aggregate / 3.51 fps (technical.city) | **3.5 - 3.8** |
| GTX 1060 6GB | 2.99 | 2.92 aggregate / 2.33 fps | **2.3 - 3.0** |
| RTX 3050 8GB | 2.40 | none sourced; [derived] 2.06 (Tom's 4070S/3060) x 1.355 (PassMark 3060/3050) = 2.8 | **2.4 - 2.8** |
| GTX 980 Ti | 2.20 | 2.20 / 2.14 | **2.1 - 2.2** |
| RTX 3060 | 1.77 | 2.06 (Tom's 62.2/30.2) | **1.8 - 2.1** |
| RTX 4060 | 1.54 | 1.54 / 1.74 (technical.city), 1.77 (Tom's) | **1.5 - 1.8** |
| Median Steam GPU (3060-4060 class) | - | - | **1.5 - 2.1** (use 1.8) |
| Radeon 780M iGPU | 4.45 | none | 4.5 (single source) |
| Intel Iris Xe | 11.6 | none | ~12 (single source) |

The synthetic and game ratios differ by up to about 25%, so quote ranges. Pick the end that is safe (the higher ratio) when setting a budget.

### 3c. Frame-time budget arithmetic

Frame time at 60 fps = 16.67 ms; at 30 fps = 33.33 ms (Unity profiling guide, see file 24).

**GPU-bound scaling.** GPU frame time on the target GPU ~ (GPU frame time on the 4070S) x r, where r is the ratio in 3b. Therefore the 4070S budget is:

`budget_4070S_ms = h x T / r`, with `T` = 16.67 or 33.33 and `h` = headroom factor.

`h = 0.80` (20% headroom for driver variation, background load, hitches and a different resolution/driver than the test; **UNSOURCED GUESS**, a choice). Usable time: 13.33 ms for 60 fps, 26.67 ms for 30 fps.

| Target hardware | r range | GPU ms on the 4070S for 60 fps | for 30 fps floor |
|---|---|---|---|
| GTX 1650 (4 GB) | 3.5 - 3.8 | **3.5 - 3.8** | 7.0 - 7.6 |
| GTX 1060 6GB / RX 580 | 2.3 - 3.0 | **4.4 - 5.8** | 8.9 - 11.6 |
| RTX 3050 8GB | 2.4 - 2.8 | 4.8 - 5.6 | 9.5 - 11.1 |
| GTX 980 Ti (Librarian-style minimum) | 2.1 - 2.2 | 6.1 - 6.3 | 12.1 - 12.7 |
| Median Steam GPU (3060-4060 class) | 1.5 - 2.1 | 6.3 - 8.9 | 12.7 - 17.8 |
| Radeon 780M iGPU | ~4.5 | 3.0 | 6.0 |
| Intel Iris Xe | ~12 | 1.1 | 2.3 (not a realistic target) |

Resolution sensitivity (pixel-bound part only): 1440p is 1.78x the pixels of 1080p, 4K is 4.0x [derived]. A card running 1440p at 60 fps needs the 4070S's 1440p GPU time <= the table value; the pixel-bound share of that 1440p time is about 1.78x its 1080p value.

**CPU scaling is different.** CPU frame time does not scale with GPU speed; it scales roughly with single-thread speed (main thread and render thread each have their own budget; frames are pipelined so FPS ~ 1 / max(main, render thread, GPU)). `budget_devCPU_ms = h x T / c`, `c` = single-thread rating of the dev CPU / that of the minimum CPU.
- PassMark single-thread (page updated 2026-10-18) [fetched] https://www.cpubenchmark.net/singleThread.html : Ryzen 3 1200 1,925; Ryzen 5 1500X 2,107; i3-6100 2,146; Ryzen 5 2600 2,233; i5-6600 2,252; i5-8400 2,364; i7-4790K 2,463; Ryzen 5 3600 2,557; i7-8700 2,624; i5-11400F 2,979; Ryzen 5 5800X3D 3,233; Ryzen 5 5600 3,254; Ryzen 5 5600X 3,366; i5-12400F 3,484; Ryzen 7 7800X3D 3,759; Ryzen 5 7600X 4,128; Ryzen 7 9800X3D 4,420.
- Against a proposed minimum of ~2,150 (Ryzen 5 1500X / i3-6100 / i5-6600 class) [derived]: dev 9800X3D c = 2.06 (6.5 ms for 60 fps); 7800X3D c = 1.75 (7.6 ms); Ryzen 5 5600X c = 1.57 (8.5 ms); Ryzen 5 3600 c = 1.19 (11.2 ms).
- **The dev CPU is not stated in the brief, so I do not assume it.** Rule: measure CPU frame time (main-thread and render-thread separately, `FrameTiming.cpuMainThreadFrameTime` / `cpuRenderThreadFrameTime`, file 24) and apply the formula with the real `c`. If the dev machine has a current top-end desktop CPU, plan on **<= 6 ms main thread and <= 6 ms render thread**; for the 30 fps floor double it.
- Single-thread scores likely understate how much faster cache-heavy CPUs (X3D) are in Unity games than an i3-6100 or Ryzen 5 1500X: UNSOURCED GUESS. Also the minimum CPU has 4 cores and sometimes 2 (i3-6100), so worker-thread-heavy work (physics, culling jobs, GPU-driver threads) has less parallelism than on the dev PC; core count also does not scale linearly.

**GPU and CPU are not additive.** The worse of the two sets fps. A scene that is 3 ms GPU / 3 ms CPU on the 4070S is fine; do not infer from low GPU ms that CPU is fine.

### 3d. Proposed target profile (concrete)

- **Minimum (1080p, 60 fps at Low/Medium, 30 fps floor at any setting): GPU GTX 1060 6GB / Radeon RX 580 8GB (PassMark ~10,000 / 8,800); 4 GB+ VRAM; CPU quad-core Intel Core i5-6600 / AMD Ryzen 5 1500X (single-thread ~2,100-2,250); 8 GB RAM; DirectX 11; Windows 10 64-bit.** Basis: it is the modal minimum in the cohort (5 of 9 list GTX 1050 or GTX 1060: GTX 1050 in Witch Cabin, Shelve the Potions and Wand & Order; GTX 1060 in Mall and Black Friday Janitor); it is below Librarian's GTX 980 Ti line; it covers about 84% of survey VRAM (>= 6 GB) and is passed by ~84% of listed Steam GPU share. [derived] The GTX 1650 (2.19%) is slower than the GTX 1060 by PassMark (7,867 vs 10,017, 0.79x); treat it as a **stretch floor** (30 fps) rather than a 60 fps promise.
- **Recommended (1080p High at 60 fps, or 1440p Medium): GPU RTX 2060 6GB / RX 5700 XT / GTX 1080 class (PassMark 14,000-16,000; r vs 4070S ~1.9-2.1); CPU 6-core Ryzen 5 3600 / Intel Core i5-10400 (~2,560 single-thread); 16 GB RAM; DirectX 11 or 12.** Basis: Librarian's recommended line is GTX 1080 / RX 5700 XT; competitor recommended GPUs run GTX 1060-1080. Note an RTX 3060 (PassMark 16,885) is ~ the median Steam GPU.
- 1440p note: 27.04% of Steam users are 1440p and 40% are 1440p-class or higher. [derived]

## 4. Confidence and unknowns

- **High:** survey shares and the cited store specs (read directly 2026-10-19). Order of the GPUs (4070S >> 3060 > 3050 > 1060 > 1650).
- **Medium:** ratios in 3b. Three independent-ish sources (PassMark, technical.city, Tom's via TechFuelHQ) agree within ~25%, but none is the requested TechPowerUp (403), technical.city mixes game sets per pair, and the Tom's numbers are a re-hosted summary I could not cross-check. The RTX 3050 ratio is derived through two sources and has no direct game-based figure.
- **Low / UNSOURCED GUESS:** the 20% headroom factor (h = 0.80); that GPU time scales linearly with the ratio for BookBoy's mix of vertex/shadow/pixel work; the URP render scale maximum; Afterburner behavior; CPU emulation tooling; the idea that Librarian/Supermarket Chaos specs are template text; the effect of 12 GB vs 4-6 GB VRAM.
- **Unknown (needs a measurement or the owner):** the dev CPU; BookBoy's own GPU/CPU ms on the 4070 Super at 1000 books (none measured here); whether the pixel-bound, vertex-bound (shadow-pass triangles from 1000 x ~440-triangle books times shadowed point lights) or CPU-bound path dominates; actual driver behaviour of `nvidia-smi -lgc/-lmc` on this card and its minimum supported clock; real hardware for a GTX 1060 / 1650 to calibrate any emulation.
- Review counts for the newest games are small (56-778 reviews for 5 of 9), so "well-reviewed" is a weak filter; two are Mixed (Plushy 51%, Mall 69%).
- Survey caveats: opt-in sample; 1440p share moved 5.2 points in one month; VRAM column includes odd shared-memory values; "Other" is 8.33% and unclassified.

## 5. Implication for BookBoy

1. **Adopt the target profile in 3d** (min GTX 1060 6GB / RX 580, i5-6600 / Ryzen 5 1500X, 8 GB RAM, DX11; recommended RTX 2060 / RX 5700 XT, Ryzen 5 3600, 16 GB). Mark GTX 1650 as a 30 fps floor.
2. **Frame-time budget on the 4070 Super at 1080p, Development Player (file 24), measured p95 not mean:**
   - **GPU <= 4.4 ms** (60 fps on GTX 1060 with 20% headroom, using the pessimistic ratio 3.0). Treat 4.4 - 5.8 ms as amber and > 5.8 ms as red. If we also want the GTX 1650 at 60, the line is **3.5 ms**. 30 fps floor on the GTX 1650: **<= 7.0 ms**.
   - **CPU main thread <= 6 ms and render thread <= 6 ms** if the dev CPU is a current high-end desktop part; recompute with 3c's formula once the dev CPU model is known.
   - This is tighter than file 24's guessed 8.3 ms p95; use these numbers instead, noting they depend on the headroom guess.
3. **Do not read the 4070S's raw 60+ fps as safe.** At 1000 books with a few shadowed point lights the 4070S will report hundreds of fps; only the ms numbers against the budget mean something. Always read GPU ms from `FrameTimingManager`/the Profiler with VSync off and the clocks locked (3e below), not fps.
4. **Emulation recipe, in order of fidelity (each answers a different question):**
   - **Render scale > 1 or 1440p/4K player resolution** (`-screen-width/-height`): multiplies pixel work by s^2. To mimic a pixel-bound r = 3.0 at 1080p use s = sqrt(3) = 1.73; r = 3.8 needs s = 1.95; native 4K is 4.0x [derived]. Represents fill rate, ROP/blend, overdraw, post-processing, MSAA cost. Does **not** represent vertex/triangle throughput or shadow-map rendering (shadow-map size is independent of screen resolution unless changed), CPU/draw-call cost, VRAM capacity, or a weak card's memory bandwidth. Needs the URP maximum verified (UNSOURCED GUESS 2.0).
   - **`nvidia-smi -lgc` (and `-lmc`) clock lock, as administrator, `-rgc/-rmc` to reset:** slows shader, ROP and (if locked) memory throughput roughly linearly with clock (UNSOURCED GUESS that scaling is close to linear). Core clock for r = 3: 2,475 MHz / 3 = ~825 MHz if the card lists a clock that low (query `-q -d SUPPORTED_CLOCKS`; GeForce floors may be higher). It keeps Ada's architecture, 12 GB VRAM and big cache, so it understates a Pascal GTX 1060's weaknesses (smaller caches, 192 GB/s bandwidth, 6 GB). Locking clocks also fixes the boost-variance noise NVIDIA warns about, so use it for all runs even without emulation.
   - **Power limit (`nvidia-smi -pl` or Afterburner):** may be limited to a small range (check `-q -d POWER`); a mild tool, not a 3x slowdown. Not recommended as the main method.
   - **Unity Quality levels** (own URP asset per level): checks that the Low tier really lowers cost (shadow resolution, soft shadows, additional-light shadows, render scale below 1, per Unity's performance page). It changes the work, not the hardware.
   - **CPU emulation:** limit logical cores/affinity at launch (UNSOURCED GUESS: Windows `start /affinity`), and compare against the formula; do not trust it as a substitute for an i3-6100-class test.
   - **A real GTX 1060/1650 or a cheap used card, or a cloud/borrowed machine, is the only test that captures VRAM limits, drivers and architecture.** Do one confirmation run on real minimum hardware before the Steam page lists the spec.
5. **Test at both 1080p and 1440p:** 47.9% of players are 1080p, 27.0% are 1440p. The recommended-spec 1440p line (RTX 2060-class, r ~2.1, 60 fps): 13.3 / 2.1 = 6.3 ms on the 4070S at 1440p.
6. **Books, lights, shadows:** because shadow-pass triangles scale with the number of books times shadowed lights (not with resolution) and are the main thing render scale cannot emulate, measure the shadow passes separately (file 24, `-perf-shadows off`). If GPU ms exceeds the budget at 1000 books, the likely cheapest cuts are fewer shadowed lights, shorter shadow distance, and culling out-of-room books (file 12), not lower resolution.
7. **Steam Deck / handheld:** Deck Verified requires 30 fps at 800p by default (1.02M pixels, 0.49x of 1080p [derived]) on a 1.6 TFLOPS 8-CU APU; the 4070S is not a representative test card for it. Plan a separate low tier (render scale ~0.7, no additional-light shadows) and test on a real Deck or treat it as post-launch. UNSOURCED GUESS: a Deck is roughly a GTX 1050-class GPU in game performance. Integrated graphics in general (8.3% of Steam, 780M r ~4.5, Iris Xe r ~12) is out of scope for the proposed minimum.
8. **Laptops are 13.8% of the survey**: "RTX 4060 Laptop" can run at 35-115 W (Notebookcheck), so budget against the minimum desktop numbers, not the laptop part names.
9. **Next measurement:** run the file 24 harness at 1080p, 1440p and render scale 1.5 / 1.75 with `-lgc` locked at the supported clock nearest 2,475/3, with 128, 500 and 1000 books, and record `gpuFrameTime`, `cpuMainThreadFrameTime` and `cpuRenderThreadFrameTime` p95 against the budget above.
