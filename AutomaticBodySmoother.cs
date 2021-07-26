using System;
using UnityEngine;

namespace HuntingSuccubus
{
    public class AutomaticBodySmoother : MVRScript
    {
        private JSONStorableFloat _tess;
        private JSONStorableFloat _tessPhong;
        private JSONStorableFloat _scanTiming;
        private int _timer = 1;

        public override void Init()
        {
            try
            {
                _tess = new JSONStorableFloat("Tess", 3.35f, 1.0f, 8.0f, true, true);
                RegisterFloat(_tess);
                CreateSlider(_tess, false);
                _tessPhong = new JSONStorableFloat("TessPhong", 0.5f, 0.0f, 1.0f, true, true);
                RegisterFloat(_tessPhong);
                CreateSlider(_tessPhong, false);
                _scanTiming = new JSONStorableFloat("Scan Timing", 100f, 10f, 1000f, true, true);
                RegisterFloat(_scanTiming);
                CreateSlider(_scanTiming, false);
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        public void Update()
        {
            try
            {
                if (_timer >= _scanTiming.val)
                {
                    foreach (Atom atom in FindObjectsOfType(typeof(Atom)))
                    {
                        if (atom.type != "Person") continue;
                        var selector = atom.GetComponentInChildren<DAZCharacterSelector>();
                        var character = selector.selectedCharacter;
                        var skin = character.skin;
                        if (skin == null) return;
                        foreach (var mat in skin.GPUmaterials)
                        {
                            if (mat != null && mat.name != "EyeReflection-1" && mat.name != "EyeReflection" && mat.name != "Irises" && mat.name != "Irises-1" && mat.name != "Eyelashes" && mat.name != "Eyelashes-1" && mat.name != "Cornea-1" && mat.name != "Cornea" && mat.name != "Sclera-1" && mat.name != "Sclera" && mat.name != "Pupils-1" && mat.name != "Pupils" && mat.name != "Tear" && mat.name != "Tear-1" && mat.name != "Teeth-1" && mat.name != "Teeth" && mat.name != "Tongue" && mat.name != "Tongue-1" && mat.name != "InnerMouth-1" && mat.name != "InnerMouth")
                            {
                                if (mat.shader.name != "Custom/Subsurface/TransparentGlossNMNoCullSeparateAlphaComputeBuff")
                                    mat.shader = Shader.Find("Custom/Subsurface/GlossNMTessMappedFixedComputeBuff");
                                mat.SetTexture("_TessTex", Texture2D.whiteTexture);
                                mat.SetFloat("_TessPhong", _tessPhong.val);
                                mat.SetFloat("_Tess", _tess.val);
                            }
                        }
                        skin.BroadcastMessage("OnApplicationFocus", true);
                    }
                    _timer = 0;
                }
                _timer += 1;
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
    }
}