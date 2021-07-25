using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MeshVR;
using SimpleJSON;


namespace HuntingSuccubus {
	public class AutomaticBodySmoother : MVRScript {
		protected JSONStorableFloat Tess;
		protected JSONStorableFloat TessPhong;
		protected JSONStorableFloat ScanTiming;
		
    private Atom _person;
    private DAZCharacterSelector _selector;
    private bool _dirty;
    private DAZCharacter _character;
	protected int timer = 1;
		public override void Init() {
			try {
				// put init code in here
				//SuperController.LogMessage("Template Loaded");

				// create custom JSON storable params here if you want them to be stored with scene JSON
				// types are JSONStorableFloat, JSONStorableBool, JSONStorableString, JSONStorableStringChooser
				// JSONStorableColor
			Tess = new JSONStorableFloat("Tess", 3.35f, 1.0f, 8.0f, true, true);
			RegisterFloat(Tess);
			CreateSlider(Tess, false);  
			TessPhong = new JSONStorableFloat("TessPhong", 0.5f, 0.0f, 1.0f, true, true);
			RegisterFloat(TessPhong);
			CreateSlider(TessPhong, false);	
			ScanTiming = new JSONStorableFloat("Scan Timing", 100f, 10f, 1000f, true, true);
			RegisterFloat(ScanTiming);
			CreateSlider(ScanTiming, false);


			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Start is called once before Update or FixedUpdate is called and after Init()
		void Start() {
			try {



			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		void Update() {
			try {
				if (timer>=ScanTiming.val)
				{
				foreach (Atom atom in FindObjectsOfType(typeof(Atom))) {
				//SuperController.LogMessage("atom="+atom.name+" atomtype="+atom.type);
								if (atom.type=="Person") {
			//SuperController.LogMessage("-----------AtomType=Person Found------------");
			
			_person = atom;
			_selector = _person.GetComponentInChildren<DAZCharacterSelector>();
			_character = _selector.selectedCharacter;
			var skin = _character.skin;
			if (skin == null) return;
			Material[] mats = skin.GPUmaterials;
			                foreach (Material mat in mats)
                {
                  //SuperController.LogMessage("-----------Material Found------------"+mat.name);
            if (mat != null && mat.name != "EyeReflection-1" && mat.name != "EyeReflection" && mat.name != "Irises" && mat.name != "Irises-1" && mat.name != "Eyelashes" && mat.name != "Eyelashes-1" && mat.name != "Cornea-1" && mat.name != "Cornea"&& mat.name != "Sclera-1"&& mat.name != "Sclera"&& mat.name != "Pupils-1"&& mat.name != "Pupils" && mat.name != "Tear" && mat.name != "Tear-1" && mat.name != "Teeth-1" && mat.name != "Teeth" && mat.name != "Tongue" && mat.name != "Tongue-1" && mat.name != "InnerMouth-1" && mat.name != "InnerMouth") {
				if(mat.shader.name != "Custom/Subsurface/TransparentGlossNMNoCullSeparateAlphaComputeBuff")
                mat.shader = Shader.Find("Custom/Subsurface/GlossNMTessMappedFixedComputeBuff");
				mat.SetTexture("_TessTex",Texture2D.whiteTexture);
				mat.SetFloat("_TessPhong", TessPhong.val);
				mat.SetFloat("_Tess", Tess.val);
				}
                }
				skin.BroadcastMessage("OnApplicationFocus", true);			
													}
			}
			timer = 0;
			}
			timer = timer+1;
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		void FixedUpdate() {
			try {
				{
				foreach (ParticleSystem mat in FindObjectsOfType(typeof(ParticleSystem))) 
					{
						//var coll = mat.collision;
						//coll.enabled = false;
					}
				}		
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		void OnDestroy() {
		}

	}
}