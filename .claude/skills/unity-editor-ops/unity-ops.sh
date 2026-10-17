#!/usr/bin/env bash
# BookBoy Unity Editor ops: preflight | save | compile | test. Fail-closed: anything unexpected exits non-zero.
# Exit codes: 0 ok, 1 real failure, 2 refused (unsafe state), 3 unparseable/unexpected output, 4 no tests ran.
set -u
PROJ="${UNITY_PROJECT_PATH:-C:/Code/BookBoy}"
echo "[unity-ops] target=$PROJ"

U() { unity command "$@" --project-path "$PROJ" --result-only; }

# jq-less JSON read: jget '<js expr over j>' ; prints value, exits 3 if parse fails or value is undefined
jget() {
  node -e 'let s="";process.stdin.on("data",d=>s+=d).on("end",()=>{let j;try{j=JSON.parse(s)}catch(e){process.exit(3)}
    let v;try{v=eval(process.argv[1])}catch(e){process.exit(3)}
    if(v===undefined)process.exit(3);console.log(typeof v==="object"?JSON.stringify(v):v)})' "$1"
}

need() { local v; v=$(printf '%s' "$2" | jget "$1") || { echo "[unity-ops] UNPARSEABLE: $1"; printf '%s\n' "$2" | head -c 600; exit 3; }; printf '%s' "$v"; }

preflight() {
  local es sc
  es=$(U editor_status) || { echo "[unity-ops] editor_status failed - Editor not reachable (Safe Mode? still loading? run: unity pipeline list)"; exit 3; }
  [ "$(need 'j.status' "$es")" = ready ] || { echo "[unity-ops] REFUSED: Editor status not ready"; exit 2; }
  [ "$(need 'j.compiling||j.domainReloadInProgress' "$es")" = false ] || { echo "[unity-ops] REFUSED: compiling/reloading - wait and retry"; exit 2; }
  echo "[unity-ops] unity=$(need 'j.unityVersion' "$es") playMode=$(need 'j.playMode' "$es")"
  PLAY=$(need 'j.playMode' "$es")
  sc=$(U list_open_scenes) || exit 3
  DIRTY=$(need 'j.scenes.filter(s=>s.isDirty).map(s=>s.path).join(",")' "$sc")
  echo "[unity-ops] dirty scenes: ${DIRTY:-none}"
}

case "${1:-}" in
  preflight) preflight ;;

  save) # rule 4/5: never write scenes in Play Mode; confirm clean after
    preflight
    [ "$PLAY" = stopped ] || { echo "[unity-ops] REFUSED: Play Mode is $PLAY - scene writes revert on exit"; exit 2; }
    [ -z "$DIRTY" ] && { echo "[unity-ops] nothing to save"; exit 0; }
    U save_all >/dev/null || exit 3
    sc=$(U list_open_scenes) || exit 3
    [ -z "$(need 'j.scenes.filter(s=>s.isDirty).length||""' "$sc")" ] || { echo "[unity-ops] FAILED: scenes still dirty after save_all"; exit 1; }
    echo "[unity-ops] saved; now verify the FILE on disk (git diff), not this message" ;;

  compile) # rule 5/19: save first (never recompile with a dirty scene), then recompile and require a clean result
    preflight
    [ -z "$DIRTY" ] || { echo "[unity-ops] REFUSED: dirty scenes ($DIRTY) - run: unity-ops.sh save"; exit 2; }
    out=$(unity recompile --project-path "$PROJ" --json) ; rc=$?
    ok=$(printf '%s' "$out" | jget 'j.success===true&&j.data.failed===false&&j.data.errors.length===0&&["up_to_date","completed"].includes(j.data.status)') || { echo "[unity-ops] UNPARSEABLE recompile output (rc=$rc)"; printf '%s\n' "$out" | head -c 800; exit 3; }
    [ "$ok" = true ] || { echo "[unity-ops] COMPILE FAILED"; printf '%s\n' "$out" | head -c 2000; exit 1; }
    echo "[unity-ops] compile OK (status=$(printf '%s' "$out" | jget 'j.data.status'))"
    # a real recompile is followed by a domain reload; commands sent during it fail with a network error
    unity status --until-ready --project-path "$PROJ" --timeout 180 >/dev/null 2>&1 || { echo "[unity-ops] Editor not ready after compile"; exit 3; }
    for _ in $(seq 1 30); do
      es=$(U editor_status 2>/dev/null) && [ "$(printf '%s' "$es" | jget 'j.status==="ready"&&!j.compiling&&!j.domainReloadInProgress' 2>/dev/null)" = true ] && break
      sleep 2
    done ;;

  test) # usage: test [editor|playmode|all] [name-filter]. Uses the LIVE Editor, never `unity test` (spawns a 2nd batch Editor -> rule 8).
    mode="${2:-editor}"; filter="${3:-}"
    case "$mode" in editor|playmode|all) ;; *) echo "[unity-ops] REFUSED: mode must be editor|playmode|all (got '$mode')"; exit 2 ;; esac
    "$0" compile || exit $?
    args=(--mode "$mode"); [ -n "$filter" ] && args+=(--filter "$filter")
    [ "$mode" = editor ] || args+=(--async_tests true)
    out=$(U run_tests "${args[@]}") || { echo "[unity-ops] run_tests call failed"; exit 3; }
    total=$(need 'j.Summary.Total' "$out"); failed=$(need 'j.Summary.Failed' "$out")
    if [ "$(printf '%s' "$out" | jget 'j.result')" = playmode_running ] && [ "$mode" != editor ]; then
      for _ in $(seq 1 150); do
        st=$(U test_status) || exit 3
        [ "$(need 'j.status' "$st")" = completed ] && break
        sleep 2
      done
      [ "$(need 'j.status' "$st")" = completed ] || { echo "[unity-ops] TIMEOUT waiting for PlayMode tests"; exit 1; }
      total=$(need 'j.summary.total' "$st"); failed=$(need 'j.summary.failed' "$st"); out="$st"
    fi
    echo "[unity-ops] total=$total failed=$failed"
    [ "$total" -gt 0 ] || { echo "[unity-ops] NO TESTS RAN (0/0 is not a pass)"; exit 4; }
    if [ "$failed" -ne 0 ]; then
      echo "[unity-ops] TESTS FAILED"
      # EditMode shape (observed): Results[].{FullName,Status,Message,StackTrace}; run_tests returns success:true even on failures
      fl=$(printf '%s' "$out" | jget '(j.Results||j.results||[]).filter(r=>(r.Status||r.status)==="Failed").map(r=>(r.FullName||r.fullName)+"\n    "+String(r.Message||r.message).trim().replace(/\s+/g," ")+"\n    "+String(r.StackTrace||r.stackTrace).split("\n")[0].trim()).join("\n")')
      if [ -n "$fl" ]; then printf '%s\n' "$fl"; else printf '%s\n' "$out" | head -c 4000; fi
      exit 1
    fi
    echo "[unity-ops] TESTS PASSED" ;;

  *) echo "usage: $0 preflight|save|compile|test [editor|playmode|all] [filter]"; exit 2 ;;
esac
