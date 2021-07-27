using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HuntingSuccubus
{
    public class AutomaticBodySmoother : MVRScript
    {
        // ReSharper disable MemberCanBePrivate.Global
        public const string TessellatedMaskValue = "Tessellated (Masked)";
        public const string TessellatedNoMaskValue = "Tessellated (Not Masked)";
        public const string TessellatedValue = "Tessellated";
        public const string NotTessellatedValue = "Skip";
        // ReSharper restore MemberCanBePrivate.Global

        public static AutomaticBodySmoother instance;

        public JSONStorableFloat tessJSON { get; private set; }
        public JSONStorableFloat tessPhongJSON { get; private set; }
        public JSONStorableBool colorizeJSON { get; private set; }

        private List<JSONStorableStringChooser> materialPolicies { get; set; }

        private JSONStorableStringChooser CreateMaterialJSON(string materialName, bool masked = false, bool active = false, bool editable = true, string label = null)
        {
            List<string> choices;
            string defaultValue;
            if (!editable)
            {
                choices = new List<string> {NotTessellatedValue};
                defaultValue = NotTessellatedValue;
            }
            else if (masked)
            {
                choices = new List<string> {TessellatedMaskValue, TessellatedNoMaskValue, NotTessellatedValue};
                defaultValue = active ? TessellatedNoMaskValue : NotTessellatedValue;
            }
            else
            {
                choices = new List<string> {TessellatedValue, NotTessellatedValue};
                defaultValue = active ? TessellatedValue : NotTessellatedValue;
            }

            var jss = new JSONStorableStringChooser(
                materialName,
                choices,
                defaultValue,
                materialName,
                (string _) => ReapplyAll());
            if (label != null) jss.label = label;
            return jss;
        }

        private readonly List<PersonWatcher> _watchers = new List<PersonWatcher>();
        private Atom _personAtom;

        public override void Init()
        {
            try
            {
                instance = this;

                materialPolicies = new List<JSONStorableStringChooser>
                {
                    CreateMaterialJSON("Face", active: true),
                    CreateMaterialJSON("Head", active: true, label: "Head (Back)"),
                    CreateMaterialJSON("Ears", active: true),
                    CreateMaterialJSON("Nostrils", active: true),
                    CreateMaterialJSON("Lips", active: true),
                    CreateMaterialJSON("Neck", active: true),
                    CreateMaterialJSON("Shoulders", masked: true, active: true),
                    CreateMaterialJSON("Forearms", active: true),
                    CreateMaterialJSON("Hands", active: true),
                    CreateMaterialJSON("Torso", masked: true, active: true),
                    CreateMaterialJSON("Nipples", active: true),
                    CreateMaterialJSON("Hips", masked: true, active: true),
                    CreateMaterialJSON("defaultMat", active: true, label: "Gens"),
                    CreateMaterialJSON("Legs", masked: true, active: true),
                    CreateMaterialJSON("Feet", active: true),
                    CreateMaterialJSON("Hips", masked: true, active: true),
                    CreateMaterialJSON("Toenails", active: true),
                    CreateMaterialJSON("Fingernails", active: true),
                    CreateMaterialJSON("Lacrimals", active: true),
                    CreateMaterialJSON("Gums", active: true),
                    CreateMaterialJSON("Teeth"),
                    CreateMaterialJSON("Tongue"),
                    CreateMaterialJSON("InnerMouth"),
                    CreateMaterialJSON("EyeReflection", editable: false),
                    CreateMaterialJSON("Pupils"),
                    CreateMaterialJSON("Tear", editable: false),
                    CreateMaterialJSON("Irises", editable: false),
                    CreateMaterialJSON("Cornea", editable: false),
                    CreateMaterialJSON("Sclera"),
                    CreateMaterialJSON("Eyelashes", editable: false),
                };

                tessJSON = new JSONStorableFloat("Tess", 3.35f, (float _) => ReapplyAll(), 1.0f, 8.0f);
                RegisterFloat(tessJSON);
                CreateSlider(tessJSON, false).label = "Tesselation";
                tessPhongJSON = new JSONStorableFloat("TessPhong", 0.5f, (float _) => ReapplyAll(), 0.0f, 1.0f);
                RegisterFloat(tessPhongJSON);
                CreateSlider(tessPhongJSON, false).label = "Phong Strength";
                colorizeJSON = new JSONStorableBool("Colorize Tessellated Areas", false, (bool _) => ReapplyAll()) {isStorable = false};
                CreateToggle(colorizeJSON);

                foreach (var materialJSON in materialPolicies)
                {
                    RegisterStringChooser(materialJSON);
                    if (materialJSON.choices.Count == 1) continue;
                    CreateScrollablePopup(materialJSON, true);
                }

                if (containingAtom.type == "Person")
                    _personAtom = containingAtom;

                OnEnable();
            }
            catch (Exception e)
            {
                SuperController.LogError("AutomaticBodySmoother failed to initialize: " + e);
            }
        }

        private void ReapplyAll()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Restore();
                watcher.Apply();
            }
        }

        public void OnEnable()
        {
            if (containingAtom == null) return;
            if (!ReferenceEquals(_personAtom, null))
            {
                WatchAtom(_personAtom);
            }
            else
            {
                SuperController.singleton.onAtomUIDsChangedHandlers += SyncAtoms;
                SyncAtoms(null);
            }

            StartCoroutine(PeriodicApplyCo());
        }

        public void OnDisable()
        {
            if (ReferenceEquals(_personAtom, null))
                SuperController.singleton.onAtomUIDsChangedHandlers -= SyncAtoms;

            foreach (var watcher in _watchers)
            {
                try
                {
                    watcher.Restore();
                }
                catch (Exception exc)
                {
                    SuperController.LogError($"AutomaticBodySmoother: Failed to restore watcher. {exc}");
                }

                Destroy(watcher);
            }

            _watchers.Clear();
        }

        private IEnumerator DelayedApplyCo(PersonWatcher watcher)
        {
            yield return 0;
            while (SuperController.singleton.isLoading)
                yield return 0;
            watcher.Apply();
        }

        private IEnumerator PeriodicApplyCo()
        {
            yield return 0;
            while (enabledJSON.val)
            {
                while (SuperController.singleton.isLoading)
                    yield return 0;

                for (var i = 0; i < _watchers.Count; i++)
                    _watchers[i].Apply();

                yield return new WaitForSeconds(1f);
            }
        }

        private void SyncAtoms(List<string> _)
        {
            foreach (var person in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
            {
                WatchAtom(person);
            }
        }

        private void WatchAtom(Atom person)
        {
            if (person.gameObject.GetComponent<PersonWatcher>() != null) return;
            var watcher = person.gameObject.AddComponent<PersonWatcher>();
            _watchers.Add(watcher);
            StartCoroutine(DelayedApplyCo(watcher));
        }

        public bool TryGetPolicy(string materialName, out string policy)
        {
            for (var i = 0; i < materialPolicies.Count; i++)
            {
                var jss = materialPolicies[i];
                if (!materialName.StartsWith(jss.name)) continue;
                policy = jss.val;
                return true;
            }

            policy = null;
            return false;
        }
    }

    public class PersonWatcher : MonoBehaviour
    {
        private DAZCharacterSelector _selector;
        private DAZSkinV2 _currentSkin;
        private readonly List<MaterialSnapshot> _materialSnapshots = new List<MaterialSnapshot>();
        private static readonly Shader _tessShader = Shader.Find("Custom/Subsurface/GlossNMTessMappedFixedComputeBuff");

        private void Awake()
        {
            _selector = gameObject.GetComponentInChildren<DAZCharacterSelector>(true);
        }

        public void Apply()
        {
            if (!ReferenceEquals(_currentSkin, null) && _selector.selectedCharacter.skin == _currentSkin) return;
            Restore();
            // ReSharper disable once Unity.NoNullPropagation
            _currentSkin = _selector.selectedCharacter?.skin;
            if (_currentSkin == null) return;
            foreach (var mat in _currentSkin.GPUmaterials)
                ApplyMaterial(mat);
            _currentSkin.BroadcastMessage("OnApplicationFocus", true);
        }

        private void ApplyMaterial(Material mat)
        {
            string policy;
            if (!AutomaticBodySmoother.instance.TryGetPolicy(mat.name, out policy))
            {
                SuperController.LogError($"Could not find any policy for material {mat.name}");
                return;
            }

            if (policy == AutomaticBodySmoother.NotTessellatedValue) return;
            var snapshot = new MaterialSnapshot
            {
                material = mat,
                shader = mat.shader,
                tess = mat.GetFloat("_Tess"),
                tessPhong = mat.GetFloat("_TessPhong"),
                color = mat.GetColor("_Color"),
                colorized = AutomaticBodySmoother.instance.colorizeJSON.val,
                tessTex = mat.GetTexture("_TessTex")
            };
            _materialSnapshots.Add(snapshot);
            mat.shader = _tessShader;
            if (policy == AutomaticBodySmoother.TessellatedNoMaskValue)
                mat.SetTexture("_TessTex", Texture2D.whiteTexture);
            mat.SetFloat("_TessPhong", AutomaticBodySmoother.instance.tessPhongJSON.val);
            mat.SetFloat("_Tess", AutomaticBodySmoother.instance.tessJSON.val);
            if (AutomaticBodySmoother.instance.colorizeJSON.val)
                mat.SetColor("_Color", Color.red);
        }

        private void OnDisable()
        {
            Restore();
        }

        public void Restore()
        {
            foreach (var materialSnapshot in _materialSnapshots.Where(materialSnapshot => materialSnapshot.material != null))
            {
                materialSnapshot.material.shader = materialSnapshot.shader;
                materialSnapshot.material.SetTexture("_TessTex", materialSnapshot.tessTex);
                materialSnapshot.material.SetFloat("_TessPhong", materialSnapshot.tessPhong);
                materialSnapshot.material.SetFloat("_Tess", materialSnapshot.tess);
                if (materialSnapshot.colorized) materialSnapshot.material.SetColor("_Color", materialSnapshot.color);
            }

            _materialSnapshots.Clear();
            _currentSkin = null;
        }

        private class MaterialSnapshot
        {
            public Material material;
            public Shader shader;
            public float tessPhong;
            public float tess;
            public Texture tessTex;
            public Color color;
            public bool colorized;
        }
    }
}