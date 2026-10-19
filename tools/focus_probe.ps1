# Logs the foreground window and the Play frame count, and tests whether a pause toggle steals focus.
# Bounded loops only (see unity-testing.md). Run from the repo root while the Editor is in Play Mode.
Add-Type @"
using System; using System.Runtime.InteropServices; using System.Text;
public class FW { [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow(); [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n); }
"@
function Fg { $sb = New-Object System.Text.StringBuilder 256; [void][FW]::GetWindowText([FW]::GetForegroundWindow(), $sb, 256); $sb.ToString() }
function Frame {
    $r = & unity command eval 'return UnityEngine.Time.frameCount + " " + UnityEngine.Application.isPlaying;' --project-path C:/Code/BookBoy --result-only 2>&1 | Out-String
    if ($r -match '"result":\s*"([^"]*)"') { $Matches[1] } else { "?" }
}
function Log($m) { "{0} | fg='{1}' | frame={2} | {3}" -f (Get-Date -Format HH:mm:ss), (Fg), (Frame), $m }

# 1. wait (up to 90 s) until Unity is not the foreground window
$unfocused = $false
for ($i = 0; $i -lt 45; $i++) {
    if ((Fg) -notlike '*Unity*') { $unfocused = $true; break }
    Start-Sleep -Seconds 2
}
if (-not $unfocused) { "Unity never lost focus within 90 s; aborting"; return }
Log "unfocused detected"
# 2. five samples with nothing done: do frames advance on their own?
for ($i = 0; $i -lt 5; $i++) { Start-Sleep -Seconds 2; Log "idle sample $i" }
# 3. toggle pause on and off through the Editor, then watch the foreground window
& unity command eval 'UnityEditor.EditorApplication.isPaused = true; return 1;' --project-path C:/Code/BookBoy --result-only | Out-Null
Log "after pause"
& unity command eval 'UnityEditor.EditorApplication.isPaused = false; return 1;' --project-path C:/Code/BookBoy --result-only | Out-Null
for ($i = 0; $i -lt 6; $i++) { Log "after unpause $i"; Start-Sleep -Seconds 1 }
