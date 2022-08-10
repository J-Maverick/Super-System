
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;
using VRC.SDKBase.Editor.BuildPipeline;

namespace ArchiTech.Editor
{
    public class ProTVBuildHelpers : IVRCSDKBuildRequestedCallback
    {

        public int callbackOrder { get { return -1; } }

        public bool OnBuildRequested(VRCSDKRequestedBuildType type)
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            UpdateVersions(scene);
            UpdateDropdowns(scene);
            UpdateAutoplayOffsets(scene);
            AutoUpgrade(scene);
            return true;
        }

        public static void UpdateVersions(Scene scene)
        {
            string versionNumber = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/ArchiTechAnon/ProTV/VERSION.md").text.Trim();
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                Text[] possibles = root.GetComponentsInChildren<Text>(true);
                foreach (Text possible in possibles)
                {
                    if (possible.gameObject.name.ToLower().Contains("protv version") && possible.text != versionNumber)
                    {
                        possible.text = versionNumber;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(possible);
                        EditorUtility.SetDirty(possible);
                    }
                }
            }
        }

        public static void UpdateDropdowns(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                Controls_ActiveState[] controls = root.GetComponentsInChildren<Controls_ActiveState>(true);
                foreach (Controls_ActiveState _control in controls)
                {
                    Controls_ActiveState control = _control;
#if !UDONSHARP // U# 0.x support
                    control = control.GetUdonSharpComponent<Controls_ActiveState>();
#endif
                    TVManagerV2 tv = control.tv;
                    if (tv == null) continue;
                    if (control.videoPlayerSwap == null) continue;
#if !UDONSHARP // U# 0.x support
                    tv = tv.GetUdonSharpComponent<TVManagerV2>();
#endif
                    control.videoPlayerSwap.ClearOptions();
                    List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
                    foreach (VideoManagerV2 _manager in tv.videoManagers)
                    {
                        VideoManagerV2 manager = _manager;
                        if (manager == null)
                        {
                            options.Add(new Dropdown.OptionData("<Missing Ref>"));
                            continue;
                        }
#if !UDONSHARP // U# 0.x support
                        manager = manager.GetUdonSharpComponent<VideoManagerV2>();
#endif
                        if (manager.customLabel != null && manager.customLabel != string.Empty)
                            options.Add(new Dropdown.OptionData(manager.customLabel));
                        else options.Add(new Dropdown.OptionData(manager.gameObject.name));
                    }
                    control.videoPlayerSwap.AddOptions(options);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(control.videoPlayerSwap);
                    EditorUtility.SetDirty(control.videoPlayerSwap);
                }
            }
        }

        public static void UpdateAutoplayOffsets(Scene scene)
        {

            GameObject[] roots = scene.GetRootGameObjects();
            List<TVManagerV2> tvs = new List<TVManagerV2>();
            List<Playlist> playlists = new List<Playlist>();
            foreach (GameObject root in roots)
            {
                TVManagerV2[] _tvs = root.GetComponentsInChildren<TVManagerV2>(true);
                tvs.AddRange(_tvs);
                Playlist[] _playlists = root.GetComponentsInChildren<Playlist>(true);
                playlists.AddRange(_playlists);
            }
            var count = 0;
            for (int i = 0; i < tvs.Count; i++)
            {
                TVManagerV2 tv = tvs[i];
#if !UDONSHARP // U# 0.x support
                tv = tv.GetUdonSharpComponent<TVManagerV2>();
#endif
                bool hasAutoplay = !string.IsNullOrWhiteSpace(tv.autoplayURL.Get()) || !string.IsNullOrWhiteSpace(tv.autoplayURLAlt.Get());
                if (!hasAutoplay)
                {
                    foreach (Playlist _playlist in playlists)
                    {
                        Playlist playlist = _playlist;
#if !UDONSHARP // U# 0.x support
                        playlist = playlist.GetUdonSharpComponent<Playlist>();
#endif
                        if (playlist.autoplayOnLoad && playlist.tv != null)
                        {
                            var _tv = playlist.tv;
#if !UDONSHARP // U# 0.x support
                            _tv = _tv.GetUdonSharpComponent<TVManagerV2>();
#endif
                            if (_tv == tv) hasAutoplay = true;
                        }
                    }
                }
                if (hasAutoplay)
                {
                    tv.autoplayStartOffset = 5f * count;
                    // Debug.Log($"{tv.transform.GetHierarchyPath()} gets autoplay start offset {5f * count}");
                    count++;
                }
                else tv.autoplayStartOffset = 0f;

#if !UDONSHARP // U# 0.x support
                tv.ApplyProxyModifications();
                PrefabUtility.RecordPrefabInstancePropertyModifications(UdonSharpEditorUtility.GetBackingUdonBehaviour(tv));
#endif
                PrefabUtility.RecordPrefabInstancePropertyModifications(tv);
                EditorUtility.SetDirty(tv);
            }
        }

        public static void AutoUpgrade(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                Controls_ActiveState[] controls = root.GetComponentsInChildren<Controls_ActiveState>(true);
                foreach (Controls_ActiveState _control in controls)
                {
                    Controls_ActiveState control = _control;
#if !UDONSHARP // U# 0.x support
                    control = control.GetUdonSharpComponent<Controls_ActiveState>();
#endif
                    if (control.updateMainUrl != null)
                    {
                        if (control.activateUrls == null)
                            control.activateUrls = control.updateMainUrl;
                        control.updateMainUrl = null;
#if !UDONSHARP // U# 0.x support
                        control.ApplyProxyModifications();
                        PrefabUtility.RecordPrefabInstancePropertyModifications(UdonSharpEditorUtility.GetBackingUdonBehaviour(control));
#endif
                        PrefabUtility.RecordPrefabInstancePropertyModifications(control);
                        EditorUtility.SetDirty(control);
                    }
                }

//                 Playlist[] playlists = root.GetComponentsInChildren<Playlist>(true);
//                 foreach (Playlist _playlist in playlists)
//                 {
//                     Playlist playlist = _playlist;
// #if !UDONSHARP // U# 0.x support
//                     playlist = playlist.GetUdonSharpComponent<Playlist>();
// #endif
//                     if (playlist.alts == null || playlist.alts.Length == 0)
//                     {
                        
//                     }

//                 }
            }
        }

    }
}