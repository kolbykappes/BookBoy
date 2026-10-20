// Play Mode check of sprinting and the sprint FOV kick (docs/unity-practices/30). Run with Lamplit.unity playing.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var pl = UnityEngine.GameObject.Find("Player"); var fpc = pl.GetComponent<StarterAssets.FirstPersonController>(); var inputs = pl.GetComponent<StarterAssets.StarterAssetsInputs>();
var sf = pl.GetComponent<SprintFov>();
var vcam = UnityEngine.GameObject.Find("PlayerFollowCamera").GetComponent<Unity.Cinemachine.CinemachineCamera>();

// ---- sprinting itself ----
var pinput = pl.GetComponent<UnityEngine.InputSystem.PlayerInput>();
var action = pinput != null && pinput.actions != null ? pinput.actions.FindAction("Sprint") : null;
bool shift = false; if (action != null) foreach (var b in action.bindings) if (b.effectivePath.ToLowerInvariant().Contains("leftshift")) shift = true;
Check("the Sprint action exists and is bound to Left Shift", action != null && shift);
Check("sprinting is faster than walking (6 against 4 m/s)", fpc.SprintSpeed > fpc.MoveSpeed && System.Math.Abs(fpc.SprintSpeed - 6f) < 0.01f && System.Math.Abs(fpc.MoveSpeed - 4f) < 0.01f, fpc.MoveSpeed + " / " + fpc.SprintSpeed);
inputs.SprintInput(true); Check("holding sprint sets the controller's sprint input", inputs.sprint); inputs.SprintInput(false); Check("releasing it clears it", !inputs.sprint);

// ---- the FOV kick ----
Check("the player has the SprintFov component with a 6 degree kick and full strength", sf != null && System.Math.Abs(sf.maxKickDegrees - 6f) < 0.01f && sf.strength == 1f);
float baseFov = sf.BaseFov;
Check("the base FOV is the lens's 58.7 vertical (90 horizontal)", System.Math.Abs(baseFov - 58.7f) < 0.05f, baseFov.ToString());
for (int i = 0; i < 90; i++) sf.Tick(0f, 0.016f);
Check("standing still: no kick, the lens is at the base FOV", System.Math.Abs(sf.CurrentKick) < 0.01f && System.Math.Abs(vcam.Lens.FieldOfView - baseFov) < 0.01f, vcam.Lens.FieldOfView.ToString());
for (int i = 0; i < 90; i++) sf.Tick(fpc.MoveSpeed, 0.016f);
Check("walking at 4 m/s: still no kick", System.Math.Abs(sf.CurrentKick) < 0.01f, sf.CurrentKick.ToString());
sf.Tick(fpc.SprintSpeed, 0.016f);
Check("the first frame of a sprint barely moves it (it eases in, it does not snap)", sf.CurrentKick > 0f && sf.CurrentKick < 1.0f, sf.CurrentKick.ToString());
for (int i = 0; i < 90; i++) sf.Tick(fpc.SprintSpeed, 0.016f);
Check("sprinting at 6 m/s for 1.4 s: the view is 6 degrees wider", System.Math.Abs(sf.CurrentKick - 6f) < 0.05f && System.Math.Abs(vcam.Lens.FieldOfView - (baseFov + 6f)) < 0.05f, sf.CurrentKick + " / " + vcam.Lens.FieldOfView);
for (int i = 0; i < 12; i++) sf.Tick(fpc.SprintSpeed * 0.5f, 0.016f);
Check("slowing down eases the kick back (between 0 and 6 after 0.2 s at half speed)", sf.CurrentKick > 0.01f && sf.CurrentKick < 5.5f, sf.CurrentKick.ToString());
for (int i = 0; i < 120; i++) sf.Tick(0f, 0.016f);
Check("stopping returns the lens to the base FOV", System.Math.Abs(vcam.Lens.FieldOfView - baseFov) < 0.02f, vcam.Lens.FieldOfView.ToString());
sf.strength = 0f; for (int i = 0; i < 90; i++) sf.Tick(fpc.SprintSpeed, 0.016f);
Check("with the strength at 0 (the player's off switch) there is no kick even at full sprint", System.Math.Abs(sf.CurrentKick) < 0.01f && System.Math.Abs(vcam.Lens.FieldOfView - baseFov) < 0.02f);
sf.strength = 1f; for (int i = 0; i < 120; i++) sf.Tick(0f, 0.016f);
sf.SetBaseFov(70f); for (int i = 0; i < 90; i++) sf.Tick(fpc.SprintSpeed, 0.016f);
Check("a new base FOV (the FOV slider) is respected: 70 + 6", System.Math.Abs(vcam.Lens.FieldOfView - 76f) < 0.05f, vcam.Lens.FieldOfView.ToString());
sf.SetBaseFov(baseFov); for (int i = 0; i < 120; i++) sf.Tick(0f, 0.016f);
// ---- the Z zoom ----
for (int i = 0; i < 120; i++) sf.Tick(0f, 0.016f);
float fov0 = vcam.Lens.FieldOfView; float rot0 = fpc.RotationSpeed;
Check("before the zoom the lens is at the base FOV and no zoom", System.Math.Abs(fov0 - baseFov) < 0.02f && sf.ZoomAmount < 0.001f);
sf.SetZoomHeld(true); sf.Tick(0f, 0.016f);
float fovFirst = vcam.Lens.FieldOfView;
Check("holding Z: the first frame has moved a little (it eases, it does not snap)", fovFirst < fov0 - 0.05f && fovFirst > fov0 * 0.8f, fovFirst.ToString());
float prev = fovFirst; bool monotone = true;
for (int i = 0; i < 60; i++) { sf.Tick(0f, 0.016f); float f = vcam.Lens.FieldOfView; if (f > prev + 1e-4f) monotone = false; prev = f; }
float expected = BookBoy.Rules.SprintFovRules.ZoomedFov(baseFov, sf.zoomFactor);
Check("it narrows smoothly (never back up) to the zoom factor: 2.5x", monotone && System.Math.Abs(vcam.Lens.FieldOfView - expected) < 0.1f, vcam.Lens.FieldOfView + " vs " + expected);
Check("the view is about 24 degrees vertical when fully zoomed", vcam.Lens.FieldOfView > 20f && vcam.Lens.FieldOfView < 28f, vcam.Lens.FieldOfView.ToString());
Check("look speed is scaled down with the zoom so aiming stays precise", fpc.RotationSpeed < rot0 * 0.6f && fpc.RotationSpeed > rot0 * 0.3f, fpc.RotationSpeed + " vs " + rot0);
sf.SetZoomHeld(false); sf.Tick(0f, 0.016f);
Check("releasing Z: the first frame widens a little (smooth out)", vcam.Lens.FieldOfView > expected && vcam.Lens.FieldOfView < baseFov - 5f, vcam.Lens.FieldOfView.ToString());
prev = vcam.Lens.FieldOfView; monotone = true;
for (int i = 0; i < 90; i++) { sf.Tick(0f, 0.016f); float f = vcam.Lens.FieldOfView; if (f < prev - 1e-4f) monotone = false; prev = f; }
Check("it widens smoothly back to the base FOV and the look speed returns", monotone && System.Math.Abs(vcam.Lens.FieldOfView - baseFov) < 0.05f && System.Math.Abs(fpc.RotationSpeed - rot0) < 0.01f, vcam.Lens.FieldOfView + " / " + fpc.RotationSpeed);
sf.SetZoomHeld(true); for (int i = 0; i < 90; i++) sf.Tick(fpc.SprintSpeed, 0.016f);
Check("zoom and sprint together: the zoom is applied to the sprint-widened view", vcam.Lens.FieldOfView < BookBoy.Rules.SprintFovRules.ZoomedFov(baseFov, sf.zoomFactor) + 5f && vcam.Lens.FieldOfView > BookBoy.Rules.SprintFovRules.ZoomedFov(baseFov, sf.zoomFactor), vcam.Lens.FieldOfView.ToString());
sf.SetZoomHeld(false); for (int i = 0; i < 150; i++) sf.Tick(0f, 0.016f);
Check("and everything is back to normal afterwards", System.Math.Abs(vcam.Lens.FieldOfView - baseFov) < 0.05f);
sb.Append(fails == 0 ? "ALL PASSED" : fails + " FAILED");
return sb.ToString();
