
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Linq;
using MeshVR;
using SimpleJSON;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Skinner.Scripts.Providers;
/// <summary>
/// MaleBodySmootherTesselletion by Hunting-Succubus
///Based on Body Shader 1.0.0, by Acid Bubbles
/// </summary>
public class MaleBodySmootherTesselletion : MVRScript
{
    private const string DefaultShaderKey = "Keep default";

    public static readonly Dictionary<string, string[]> GroupedMaterials = new Dictionary<string, string[]>
    {
		
		{
            "Tess",
             new[]
            {
                "defaultMat",
                "Anus",
				"Anus",
                "Ears",
                "Face",
                "Feet",
                "Forearms",
				"Genitalia",
				"Genitalia-1",
                "Hands",
                "Head",
                "Hips",
                "Legs",
                "Neck",
                "Nipples",
                "Nostrils",
                "Lips",
                "Shoulders",
                "Torso",
				"Torso-1",
				"Torso-2",
				"Genitals",
				"Hips-1",
			
				"Hidden",
				"Anus",
				"Anus-1",
				"Genital",
				"Anus-1",
                "Ears-1",
                "Face-1",
                "Feet-1",
                "Forearms-1",
                "Hands-1",
                "Head-1",
                "Hips-1",
                "Legs-1",
                "Neck-1",
                "Nipples-1",
                "Nostrils-1",
                "Lips-1",
                "Shoulders-1",
                "Torso-1",


				"Hidden-1",
				"Genital-1",
				"Anus-2",
                "Ears-2",
                "Face-2",
                "Feet-2",
                "Forearms-2",
                "Hands-2",
                "Head-2",
                "Hips-3",
                "Legs-2",
                "Neck-2",
                "Nipples-2",
                "Nostrils-2",
                "Lips-2",
                "Shoulders-2",
                "Torso-2",

				"Hips-2",
				"Hidden-2"

				
            }
		},


        };
    private Atom _person;
    private DAZCharacterSelector _selector;
    private bool _dirty;
    private DAZCharacter _character;
    private Dictionary<Material, Shader> _original;
    private List<MapSettings> _map = new List<MapSettings>();
    private DAZHairGroup _hair;
    private JSONStorableStringChooser _applyToJSON;
    private JSONStorableStringChooser _shaderJSON;
    private JSONStorableFloat _alphaJSON;
    private JSONStorableFloat _renderQueue;

    private class MapSettings
    {
        public string ShaderName { get; set; }
        public string MaterialName { get; set; }
        public float Alpha { get; set; }
        public int RenderQueue { get; set; }
    }

    public override void Init()
    {
        try
        {
            if (containingAtom?.type != "Person")
            {
                SuperController.LogError($"This plugin only works on Person atoms");
                DestroyImmediate(this);
                return;
            }

            _person = containingAtom;
            _selector = _person.GetComponentInChildren<DAZCharacterSelector>();
			/////////////////////////////////////////////////////////////////////
			
				JSONStorable geometry = containingAtom.GetStorableByID("geometry");
				DAZCharacterSelector dcs = geometry as DAZCharacterSelector;
				
				//

				
				//
				
            InitSettings();
            InitControls();

            _dirty = true;
        }
        catch (Exception e)
        {
            SuperController.LogError("Failed to init: " + e);
            DestroyImmediate(this);
        }
    }

    private void InitSettings()
    {
        foreach (var mat in GroupedMaterials.Values.SelectMany(v => v).Distinct())
        {
            var settings = new MapSettings { MaterialName = mat };
            _map.Add(settings);
        }
    }

    private void InitControls()
    {
        try
        {
			//

			//
			
			
			
			
            var refreshShadersJSON = new JSONStorableAction("Refresh shaders", () => _applyToJSON.choices = ScanShaders());
            //CreateButton("Refresh shaders", false).button.onClick.AddListener(() => _shaderJSON.choices = ScanShaders());

            var groups = GroupedMaterials.Keys.ToList();
            _applyToJSON = new JSONStorableStringChooser("Apply to...", groups, groups.FirstOrDefault(), "Apply to...");
            var applyToPopup = CreateScrollablePopup(_applyToJSON, false);
            applyToPopup.popupPanelHeight = 1200f;

            _shaderJSON = new JSONStorableStringChooser("Shader", ScanShaders(), DefaultShaderKey, $"Shader", (string val) => ApplyToGroup());
			
			
			
            _shaderJSON.storeType = JSONStorableParam.StoreType.Physical;
            var shaderPopup = CreateScrollablePopup(_shaderJSON, true);
            shaderPopup.popupPanelHeight = 1200f;
            // TODO: Find a way to see the full names when open, otherwise it's useless. Worst case, only keep the end.
            // linkPopup.labelWidth = 1200f;

            _alphaJSON = new JSONStorableFloat($"Alpha", 0f, (float val) => ApplyToGroup(), -1f, 1f);
            //CreateSlider(_alphaJSON, true);

            _renderQueue = new JSONStorableFloat($"Render Queue", 1999f, (float val) => ApplyToGroup(), -1f, 5000f);
            //CreateSlider(_renderQueue, true);
        }
        catch (Exception e)
        {
            SuperController.LogError("Failed to init controls: " + e);
        }
    }

    private static List<string> ScanShaders()
    {
        return UnityEngine.Resources
            .FindObjectsOfTypeAll(typeof(Shader))
            .Cast<Shader>()
            .Where(s => s != null)
            .Select(s => s.name)
            .Where(s => !string.IsNullOrEmpty(s) && !s.StartsWith("__"))
            .OrderBy(s => s)
            .ToList();
    }

    private void ApplyToGroup()
    {
        var group = _applyToJSON.val;
        string[] materialNames;
        if (!GroupedMaterials.TryGetValue(group, out materialNames))
            return;

        foreach (var materialName in materialNames)
        {
            var setting = _map.FirstOrDefault(m => m.MaterialName == materialName);
            if (setting == null) continue;
			setting.ShaderName = _shaderJSON.val;
            setting.ShaderName = "Sprites/Mask";
            setting.Alpha = _alphaJSON.val;
            setting.RenderQueue = (int)Math.Round(_renderQueue.val);
        }

        _dirty = true;
    }
	
	    private void ApplyToGroupTess()
    {
        var group = "Glass";
        string[] materialNames;
        if (!GroupedMaterials.TryGetValue(group, out materialNames))
            return;

        foreach (var materialName in materialNames)
        {
            var setting = _map.FirstOrDefault(m => m.MaterialName == materialName);
            if (setting == null) continue;
            setting.ShaderName = _shaderJSON.val;
            setting.Alpha = _alphaJSON.val;
            setting.RenderQueue = (int)Math.Round(_renderQueue.val);
        }

        _dirty = true;
    }

    public void Update()
    {
        try
        {
            if (!_dirty)
            {
                if (_selector.selectedCharacter != _character)
                    _dirty = true;
              // else if (_selector.selectedHairGroup != _hair)
                    _dirty = true;

                return;
            }

            // if (shaderJSON.val == DefaultShaderKey)
            // {
            //     if (_original != null)
            //     {
            //         foreach (var x in _original)
            //             x.Key.shader = x.Value;
            //     }
            //     _dirty = false;
            //     return;
            // }

            _character = _selector.selectedCharacter;
            if (_character == null) return;
            var skin = _character.skin;

            if (skin == null) return;
            //_hair = _selector.selectedHairGroup;

            // SuperController.LogMessage(string.Join(", ", skin.GPUmaterials.Select(m => m.name).OrderBy(n => n).ToArray()));
            // SuperController.LogMessage(string.Join(", ", _map.Select(m => m.Key).OrderBy(n => n).ToArray()));

            if (_original == null)
            {
                _original = new Dictionary<Material, Shader>();
                foreach (var mat in skin.GPUmaterials)
                {
                    _original.Add(mat, mat.shader);
                }
            }

            foreach (var setting in _map)
            {
                if (setting.ShaderName == DefaultShaderKey || setting.ShaderName == null) ;
			var shader = Shader.Find("Custom/Subsurface/GlossNMTessMappedFixedComputeBuff");
			//CustomSubsurfaceAlphaMaskComputeBuff
			//Custom/Subsurface/TransparentGlossNoCullSeparateAlphaComputeBuff
			//GPUToolsMeshedVRHair
			//
                if (shader == null) return;
                var mat = skin.GPUmaterials.FirstOrDefault(x => x.name == setting.MaterialName);
                if (mat == null) continue;
                //mat.shader = shader;
				//setting it later
               // mat.SetFloat("_AlphaAdjust", 0);
                //mat.renderQueue = 1999;
            }
						        Material[] mats = skin.GPUmaterials;
        foreach (Material mat in mats) {
            if (mat != null && mat.name != "EyeReflection-1" && mat.name != "EyeReflection" && mat.name != "Irises" && mat.name != "Irises-1" && mat.name != "Eyelashes" && mat.name != "Eyelashes-1" && mat.name != "Cornea-1" && mat.name != "Cornea"&& mat.name != "Sclera-1"&& mat.name != "Sclera"&& mat.name != "Pupils-1"&& mat.name != "Pupils" && mat.name != "Tear" && mat.name != "Tear-1" && mat.name != "Teeth-1" && mat.name != "Teeth" && mat.name != "Tongue" && mat.name != "Tongue-1" && mat.name != "InnerMouth-1" && mat.name != "InnerMouth") {
                //SuperController.LogMessage("Skin has material named " + mat.name);
                mat.shader = Shader.Find("Custom/Subsurface/GlossNMTessMappedFixedComputeBuff");
				//mat.mainTexture = null;
				mat.SetTexture("_TessTex",Texture2D.whiteTexture);
				mat.SetFloat("_TessPhong", 0.5f);
				//mat.SetFloat("_Tess", 8f);
            }
        }
		
		
		
		
		
		
		
		
		

            // var hairMaterial = _hair?.GetComponentInChildren<MeshRenderer>()?.material;
            // if (hairMaterial != null)
            // {
            //     hairMaterial.shader = shader;
            // }

            skin.BroadcastMessage("OnApplicationFocus", true);
            _dirty = false;
        }
        catch (Exception e)
        {
            SuperController.LogError("something failed: " + e);
        }

    }

    public void OnDisable()
    {
        try
        {
            _dirty = false;
            if (_original != null)
            {
                foreach (var x in _original)
                    x.Key.shader = x.Value;
            }
            _character?.skin?.BroadcastMessage("OnApplicationFocus", true);
        }
        catch (Exception e)
        {
           // SuperController.LogError("something failed: " + e);
        }
    }

    public void OnDestroy()
    {
        OnDisable();
    }
}

