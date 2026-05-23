//setting the materials and shaders variables with use of UI in playmode

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace WorldSpaceTransitions
{
    public class FadingTransitionDemo : MonoBehaviour
    {
        private FadingTransition.SurfaceType surfaceType = FadingTransition.SurfaceType.plane;
        private FadingTransition.FadingCentre fadingCentre = FadingTransition.FadingCentre.gizmo;

        private float transitionSpread = 1f;
        [Header("Get Settings From UI")]
        public bool getFromUI = true;
        [Space(10)]

        public Material[] transitionMaterials;//materials to change shaders and properties

        private Dictionary<Material, Shader> startMaterialShaders;
        private Dictionary<Material, Material> originalMaterials;

        [Space(10)]
        public Slider radiusOrDistanceSlider;
        public Text radiusOrDistanceValueText;
        public Text radiusOrDistanceTitle;
        public Slider spreadSlider;
        public Toggle invertToggle;
        public Text spreadValue;
        public Slider noiseScaleWorldSpaceSlider;
        public Text noiseScaleWorldSpaceVal;
        public Slider noiseScaleScreenSpaceSlider;
        public Text noiseScaleScreenSpaceVal;
        public Dropdown surfaceDropdown;
        public Dropdown orientationDropdown;
        public Dropdown materialDropdown;

        public Toggle triplanarMappingToggle;
        private bool triplanarMapping = true;

        public Dropdown screenspaceTextures;
        //public Dropdown atlasTextures;
        public Dropdown _3DTextures;
        public Dropdown triplanarTextures;

        private Vector3 gizmoPos;
        private Quaternion gizmoRot;
        private Transform gizmo;
        private float radius = 3f;
        private float radiusMax = 4.5f;
        private float distance = 0;//this is a transition plane distance from camera when in "tied to camera" mode.

        enum SurfaceType
        {
            Opaque,
            Transparent
        }

        enum BlendMode
        {
            Alpha,
            Premultiply,
            Additive,
            Multiply
        }


        public static FadingTransitionDemo instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
            }
        }

        void Start()
        {
            //get starting values from FadingTransition.instance
            surfaceType = FadingTransition.instance.surfaceType;
            fadingCentre = FadingTransition.instance.fadingCentre;
            //

            //store initial material values
            originalMaterials = new Dictionary<Material, Material>();
            startMaterialShaders = new Dictionary<Material, Shader>();

            foreach (Material m in transitionMaterials)
            {
                originalMaterials.Add(m, new Material(m));
                startMaterialShaders.Add(m, m.shader);
            }
            //
            if (FindObjectOfType<GizmoFollow>())
            {
                gizmo = FindObjectOfType<GizmoFollow>().transform;
                if (gizmo)
                {
                    gizmoPos = gizmo.position;
                    gizmoRot = gizmo.rotation;
                }
            }
            if (!enabled) return;
            if (orientationDropdown)
            {
                SwitchCenter(orientationDropdown.value);//get initial values from UI
                orientationDropdown.onValueChanged.AddListener(SwitchCenter);
            }
            else
            {
                SwitchCenter((int)fadingCentre);
            }


            if (radiusOrDistanceSlider)
            {
                if (getFromUI)
                {
                    radius = radiusOrDistanceSlider.value;//get initial values from UI
                    radiusMax = radiusOrDistanceSlider.maxValue;//get initial values from UI
                }
                radiusOrDistanceSlider.onValueChanged.AddListener(SetValue);
                radiusOrDistanceSlider.transform.parent.gameObject.SetActive((fadingCentre == FadingTransition.FadingCentre.camera) || (surfaceType == FadingTransition.SurfaceType.sphere));
            }

            if (spreadSlider)
            {
                SetSpread(spreadSlider.value);//get initial values from UI
                spreadSlider.onValueChanged.AddListener(SetSpread);
                spreadValue.text = spreadSlider.value.ToString("0.000");
            }

            if (invertToggle)
            {
                InvertTransition(invertToggle.isOn);//get initial values from UI
                invertToggle.onValueChanged.AddListener(InvertTransition);
            }

            if (triplanarMappingToggle)
            {
                if (getFromUI) 
                {
                    SetTriplanarMapping(triplanarMappingToggle.isOn);//get initial values from UI
                }
                else
                {
                    triplanarMappingToggle.isOn = FadingTransition.instance.useTriplanarNoise;
                }
                triplanarMappingToggle.onValueChanged.AddListener(SetTriplanarMapping);
            }

            if (noiseScaleWorldSpaceSlider)
            {
                if (getFromUI)
                {
                    SetNoiseScale(noiseScaleWorldSpaceSlider.value);//get initial values from UI
                }
                else 
                {
                    noiseScaleWorldSpaceSlider.value = FadingTransition.instance.noiseScaleWorld;
                }
                noiseScaleWorldSpaceSlider.onValueChanged.AddListener(SetNoiseScale);
            }
            if (noiseScaleScreenSpaceSlider)
            {
                if (getFromUI)
                {
                    SetNoiseScreenSpaceScale(noiseScaleScreenSpaceSlider.value);//get initial values from UI
                }
                else
                {
                    noiseScaleScreenSpaceSlider.value = FadingTransition.instance.noiseScaleScreen;
                }
                    noiseScaleScreenSpaceSlider.onValueChanged.AddListener(SetNoiseScreenSpaceScale);
            }
            if (surfaceDropdown)
            {
                
                SwitchMode(surfaceDropdown.value);//get initial values from UI
                surfaceDropdown.onValueChanged.AddListener(SwitchMode);
            }
            else
            {
                SwitchMode(0);
            }

            if (materialDropdown)
            {
                ModifyMaterials(materialDropdown.value);//get initial values from UI
                materialDropdown.onValueChanged.AddListener(ModifyMaterials);
            }

        }

        void OnEnable()
        {
            if (surfaceType == FadingTransition.SurfaceType.plane)
            {
                Shader.EnableKeyword("FADE_PLANE");
                Shader.EnableKeyword("CLIP_PLANE");
            }
            if (surfaceType == FadingTransition.SurfaceType.sphere)
            {
                Shader.EnableKeyword("FADE_SPHERE");
                Shader.EnableKeyword("CLIP_SPHERE");
            }
        }

        void OnDisable()
        {
            Shader.DisableKeyword("FADE_PLANE");
            Shader.DisableKeyword("CLIP_PLANE");
            Shader.DisableKeyword("FADE_SPHERE");
            Shader.DisableKeyword("CLIP_SPHERE");

            foreach (Material m in transitionMaterials)
            {
                try
                {
                    m.shader = startMaterialShaders[m];
                    if (m.name.Contains("transp")) 
                    { 
                        Debug.Log("transp"); 

                    }
                    m.CopyPropertiesFromMaterial(originalMaterials[m]);

                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e);
                }
            }//bring back the initial material properties
        }

        void OnApplicationQuit()
        {
            if (!enabled) return;
            foreach (Material m in transitionMaterials)
            {
                try
                {
                    m.shader = startMaterialShaders[m];
                    m.CopyPropertiesFromMaterial(originalMaterials[m]);

                }
                catch(System.Exception e)
                {
                    Debug.LogWarning(e);
                }
                    
            }//bring back the initial material properties
        }


        public void switchToSphere()
        {
            surfaceType = FadingTransition.SurfaceType.sphere;
            Shader.DisableKeyword("FADE_PLANE");
            Shader.DisableKeyword("CLIP_PLANE");
            Shader.EnableKeyword("FADE_SPHERE");
            Shader.EnableKeyword("CLIP_SPHERE");
            Shader.SetGlobalInt("_FADE_PLANE", 0);
            Shader.SetGlobalInt("_FADE_SPHERE", 1);
            if (fadingCentre == FadingTransition.FadingCentre.camera)
            {
                gizmo.localPosition = Vector3.zero;
            }
            Shader.SetGlobalFloat("_Radius", (fadingCentre == FadingTransition.FadingCentre.gizmo) ? radius : distance);
            radiusOrDistanceSlider.transform.parent.gameObject.SetActive(true);
            radiusOrDistanceTitle.text = "sphere radius";
        }

        public void switchToPlane()
        {
            surfaceType = FadingTransition.SurfaceType.plane;
            Shader.DisableKeyword("FADE_SPHERE");
            Shader.DisableKeyword("CLIP_SPHERE");
            Shader.EnableKeyword("FADE_PLANE");
            Shader.EnableKeyword("CLIP_PLANE");
            Shader.SetGlobalInt("_FADE_PLANE", 1);
            Shader.SetGlobalInt("_FADE_SPHERE", 0);
            if (fadingCentre == FadingTransition.FadingCentre.camera)
            {
                gizmo.localPosition = new Vector3(0, 0, distance);
            }
            radiusOrDistanceSlider.transform.parent.gameObject.SetActive(fadingCentre == FadingTransition.FadingCentre.camera);
            radiusOrDistanceTitle.text = "plane distance";

        }

        public void CenterToCamera()
        {
            fadingCentre = FadingTransition.FadingCentre.camera;

            gizmoPos = gizmo.position;//store the gizmo position
            gizmoRot = gizmo.rotation;//store the gizmo rotation

            Plane sPlane = new Plane(gizmo.forward, gizmo.position);//fading transition plane
            float planeDist = sPlane.GetDistanceToPoint(Camera.main.transform.position);

            if (radiusOrDistanceSlider)
            {
                radiusOrDistanceSlider.maxValue = 1.5f * planeDist;
                radiusOrDistanceSlider.value = planeDist;
                radiusOrDistanceValueText.text = radiusOrDistanceSlider.value.ToString("0.000");
            }
            distance = planeDist;
            gizmo.SetParent(Camera.main.transform);
            gizmo.localRotation = Quaternion.Euler(0, 180, 0);
            gizmo.localPosition = new Vector3(0, 0, (surfaceType == FadingTransition.SurfaceType.plane) ? planeDist : 0);//move the gizmo on the fading plane to the closest to camera point
            foreach (Transform t in gizmo) t.gameObject.SetActive(false);
            radiusOrDistanceSlider.transform.parent.gameObject.SetActive(true);
            radiusOrDistanceTitle.text = (surfaceType == FadingTransition.SurfaceType.plane) ? "plane distance" : "sphere radius";
        }

        public void CenterToGizmo()
        {
            if (!gizmo) return;

            fadingCentre = FadingTransition.FadingCentre.gizmo;
            gizmo.SetParent(null);
            gizmo.position = gizmoPos;
            gizmo.rotation = gizmoRot;
            foreach (Transform t in gizmo) t.gameObject.SetActive(true);
            if (radiusOrDistanceSlider) radiusOrDistanceSlider.transform.parent.gameObject.SetActive(surfaceType == FadingTransition.SurfaceType.sphere);
            foreach (Transform t in gizmo) t.gameObject.SetActive(true);
        }

        void SetValue(float val)
        {
            radiusOrDistanceValueText.text = val.ToString("0.000");
            if ((surfaceType == FadingTransition.SurfaceType.plane) && (fadingCentre == FadingTransition.FadingCentre.camera))
            {
                if (gizmo) gizmo.localPosition = new Vector3(0, 0, val);
                distance = val;
            }
            else
            {
                Shader.SetGlobalFloat("_Radius", val);
                if (fadingCentre == FadingTransition.FadingCentre.gizmo) radius = val;
            }
        }

        public void SetSpread(float val)
        {
            val = Mathf.Clamp(val, 0.00001f, Mathf.Infinity);
            Shader.SetGlobalFloat("_spread", val);
            spreadValue.text = spreadSlider.value.ToString("0.000");
        }

        void SetNoiseScale(float val)
        {
            Shader.SetGlobalFloat("_Noise3dScale", val);
            noiseScaleWorldSpaceVal.text = val.ToString("0.000");
        }

        void SetNoiseScreenSpaceScale(float val)
        {
            Shader.SetGlobalFloat("_ScreenNoiseScale", val);
            noiseScaleScreenSpaceVal.text = val.ToString("0.000");
        }

        public void SwitchCenter(int i)
        {
            switch (i)
            {
                case 1:
                    CenterToCamera();
                    break;
                case 0:
                    CenterToGizmo();
                    break;
                default:
                    break;
            }

        }

        public void SwitchMode(int i)
        {
            switch (i)
            {
                case 0:
                    switchToPlane();
                    break;
                case 1:
                    switchToSphere();
                    break;
                default:
                    break;
            }
        }

        public void ModifyMaterials(int i)
        {
            string s = materialDropdown.captionText.text;
            Shader shader = Shader.Find("");

            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                Debug.Log("null");
                if (s == "transparent one sided") shader = Shader.Find("Fading/Surface/Transparent");
                if (s == "transparent two sided") shader = Shader.Find("Fading/Surface/Transparent");
                if (s == "dissolve one sided") shader = Shader.Find("Fading/Surface/Dissolve");
                if (s == "dissolve two sided") shader = Shader.Find("Fading/Surface/Dissolve");
                if (s == "screenspace dissolve 1/s") shader = Shader.Find("Fading/Surface/ScreenFading");
                if (s == "screenspace dissolve 2/s") shader = Shader.Find("Fading/Surface/ScreenFading");
                if (s == "dissolve glow 1/s") shader = Shader.Find("Fading/Surface/DissolveGlow");
                if (s == "dissolve glow 2/s") shader = Shader.Find("Fading/Surface/DissolveGlow");
                if (s == "screensp. diss. glow 1/s") shader = Shader.Find("Fading/Surface/ScreenspaceDissolveGlow");
                if (s == "screensp. diss. glow 2/s") shader = Shader.Find("Fading/Surface/ScreenspaceDissolveGlow");
                if (s == "dissolve m.b. one sided") shader = Shader.Find("Fading/Surface/Dissolve/MetalBumped");
                if (s == "scrsp. dissolve m.b. 1/s") shader = Shader.Find("Fading/Surface/ScreenFading/MetalBumped");

                //Hide/show the adequate shader global variables/keywords

                if (noiseScaleWorldSpaceSlider)
                {
                    noiseScaleWorldSpaceSlider.transform.parent.gameObject.SetActive(s.Contains("diss") && !s.Contains("scr"));

                }
                if (noiseScaleScreenSpaceSlider)
                {
                    noiseScaleScreenSpaceSlider.transform.parent.gameObject.SetActive(s.Contains("diss") && s.Contains("scr"));

                }

                if (triplanarMappingToggle)
                {
                    triplanarMappingToggle.gameObject.SetActive(s.Contains("diss") && !s.Contains("scr"));
                }

                foreach (Material m in transitionMaterials)
                {
                    m.shader = shader;
                    if (m.HasProperty("_Cull")) m.SetFloat("_Cull", (s.Contains("two") || s.Contains("2/s")) ? 0 : 2);
                }
                //return;
                if (triplanarTextures)
                {
                    triplanarTextures.gameObject.SetActive(s.Contains("diss") && !s.Contains("scr") && triplanarMapping);
                }
                if (screenspaceTextures)
                {
                    screenspaceTextures.gameObject.SetActive(s.Contains("diss") && s.Contains("scr"));
                }
                if (_3DTextures)
                {
                    _3DTextures.gameObject.SetActive(s.Contains("diss") && !s.Contains("scr") && !triplanarMapping);
                }
            }
            else if (GraphicsSettings.defaultRenderPipeline.name.Contains("URP"))
            {
                if (s == "transparent one sided") shader = Shader.Find("FadingURP/Transparent");
                if (s == "dissolve") shader = Shader.Find("FadingURP/Dissolve");
                if (s == "dissolve double sided") shader = Shader.Find("FadingURP/Dissolve");
                if (s == "screenspace dissolve") shader = Shader.Find("FadingURP/ScreenspaceDissolve");
                if (s == "screensp. diss. double") shader = Shader.Find("FadingURP/ScreenspaceDissolve");
                if (s == "dissolve glow 1/s") shader = Shader.Find("FadingURP/DissolveGlow");
                if (s == "dissolve glow 2/s") shader = Shader.Find("FadingURP/DissolveGlow");
                if (s == "screensp. diss. glow 1/s") shader = Shader.Find("FadingURP/ScreenspaceDissolveGlow");
                if (s == "screensp. diss. glow 2/s") shader = Shader.Find("FadingURP/ScreenspaceDissolveGlow");

                //Hide/show the adequate shader global variables/keywords

                if (noiseScaleWorldSpaceSlider)
                {
                    noiseScaleWorldSpaceSlider.transform.parent.gameObject.SetActive(s.Contains("diss") && !s.Contains("scr"));

                }
                if (noiseScaleScreenSpaceSlider)
                {
                    noiseScaleScreenSpaceSlider.transform.parent.gameObject.SetActive(s.Contains("diss") && s.Contains("scr"));

                }

                if (triplanarMappingToggle)
                {
                    triplanarMappingToggle.gameObject.SetActive(s.Contains("diss") && !s.Contains("scr"));
                }

                if (screenspaceTextures)
                {
                    screenspaceTextures.gameObject.SetActive(s.Contains("diss") && s.Contains("scr"));
                }
                if (triplanarTextures)
                {
                    triplanarTextures.gameObject.SetActive(s.Contains("diss") && !s.Contains("scr") && triplanarMapping);
                }
                if (_3DTextures)
                {
                    _3DTextures.gameObject.SetActive(s.Contains("diss") && !s.Contains("scr") && !triplanarMapping);
                }

                foreach (Material m in transitionMaterials)
                {
                    m.shader = shader;
                    m.SetFloat("_Cull", (s.Contains("double") || s.Contains("2/s")) ? 0 : 2);
                    if (s.Contains("transp"))
                    {
                        m.SetFloat("_Surface", (float)SurfaceType.Transparent);
                        m.SetFloat("_Blend", (float)BlendMode.Alpha);
                        m.SetFloat("_AlphaClip", 1);
                    }
                    else
                    {
                        m.SetFloat("_Surface", (float)SurfaceType.Opaque);
                        //m.SetFloat("_AlphaClip", 1);
                    }
                    SetupMaterialBlendMode(m);
#if UNITY_EDITOR
                    if (s.Contains("transp"))
                    {
                        string apath = AssetDatabase.GetAssetPath(m);
                        AssetDatabase.ImportAsset(apath);
                    }
#endif
                }
            }

        }

        public void InvertTransition(bool val)
        {
            foreach (Material m in transitionMaterials)
            {
                if (m.HasProperty("_inverse")) m.SetFloat("_inverse", val ? 1 : 0);
            }
        }

        public void SetTriplanarMapping(bool val)
        {
            triplanarMapping = val;
            if (val)
            {
                Shader.EnableKeyword("NOISETRIPLANAR");
                Shader.SetGlobalInt("_NOISETRIPLANAR", 1);
            }
            else
            {
                Shader.DisableKeyword("NOISETRIPLANAR");
                Shader.SetGlobalInt("_NOISETRIPLANAR", 0);
            }
            //if (atlasTextures) atlasTextures.gameObject.SetActive(!val);
            if (_3DTextures) _3DTextures.gameObject.SetActive(!val);
            if (triplanarTextures) triplanarTextures.gameObject.SetActive(val);

        }

        void SetupMaterialBlendMode(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            bool alphaClip = material.GetFloat("_AlphaClip") == 1;
            if (alphaClip)
                material.EnableKeyword("_ALPHATEST_ON");
            else
                material.DisableKeyword("_ALPHATEST_ON");

            SurfaceType surfaceType = (SurfaceType)material.GetFloat("_Surface");
            if (surfaceType == 0)
            {
                material.SetOverrideTag("RenderType", "");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                material.SetShaderPassEnabled("ShadowCaster", true);
            }
            else
            {
                BlendMode blendMode = (BlendMode)material.GetFloat("_Blend");
                switch (blendMode)
                {
                    case BlendMode.Alpha:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                    case BlendMode.Premultiply:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                    case BlendMode.Additive:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                    case BlendMode.Multiply:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                }
            }
        }

    }
}