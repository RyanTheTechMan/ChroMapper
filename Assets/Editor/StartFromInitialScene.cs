using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad] //a "simplified" version of https://github.com/marijnz/unity-toolbar-extender/ (I know this is long. but like, we don't have to touch this)
public class StartFromInitialScene : MonoBehaviour
{
    static int m_toolCount;
    static GUIStyle m_commandStyle = null;
    
    public static GUIStyle commandButtonStyle;
    
    public static readonly List<Action> LeftToolbarGUI = new List<Action>();
    
    static StartFromInitialScene()
    {
	    Type toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        string fieldName = "k_ToolCount";
        FieldInfo toolIcons = toolbarType.GetField(fieldName,BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        m_toolCount = toolIcons != null ? (int) toolIcons.GetValue(null) : 7;
        ToolbarCallback.OnToolbarGUI -= OnGUI;
        ToolbarCallback.OnToolbarGUI += OnGUI;

        Setup();
    }

    private static void Setup()
    {
	    LeftToolbarGUI.Add(SwitchAndPlay);
    }
    
    static void SwitchAndPlay()//◀◁▶▷►
    {
	    GUILayout.FlexibleSpace();

	    if(GUILayout.Button(new GUIContent("◀", "Start from Initial Scene"), commandButtonStyle))
	    {
		    SceneHelper.StartScene("Assets/__Scenes/00_FirstBoot.unity");
	    }
    }
    
    static void OnGUI()
		{
			commandButtonStyle = new GUIStyle("Command")
			{
				fontSize = 12,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				fontStyle = FontStyle.Bold
			};
			if (m_commandStyle == null) m_commandStyle = new GUIStyle("CommandLeft");
			
			var screenWidth = EditorGUIUtility.currentViewWidth;

			// Following calculations match code reflected from Toolbar.OldOnGUI()
			float playButtonsPosition = (screenWidth - 250) / 2;

			Rect leftRect = new Rect(0, 0, screenWidth, Screen.height);
			leftRect.xMin += 10; // Spacing left
			leftRect.xMin += 32 * m_toolCount; // Tool buttons
			leftRect.xMin += 20; // Spacing between tools and pivot
			leftRect.xMin += 64 * 2; // Pivot buttons
			leftRect.xMax = playButtonsPosition;

			// Add spacing around existing controls
			leftRect.xMin += 10;
			leftRect.xMax -= 10;

			// Add top and bottom margins
			leftRect.y = 5;
			leftRect.height = 24;

			if (leftRect.width > 0)
			{
				GUILayout.BeginArea(leftRect);
				GUILayout.BeginHorizontal();
				foreach (var handler in LeftToolbarGUI)
				{
					handler();
				}
				GUILayout.EndHorizontal();
				GUILayout.EndArea();
			}
		}
	}

static class SceneHelper
{
	static string sceneToOpen;

	public static void StartScene(string scene)
	{
		if(EditorApplication.isPlaying) EditorApplication.isPlaying = false;

		sceneToOpen = scene;
		EditorApplication.update += OnUpdate;
	}

	static void OnUpdate()
	{
		if (sceneToOpen == null || EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
		

		EditorApplication.update -= OnUpdate;

		if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
		{
			EditorSceneManager.OpenScene(sceneToOpen);
			EditorApplication.isPlaying = true;
		}
		sceneToOpen = null;
	}
}

public static class ToolbarCallback
{
	static Type m_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
	static Type m_guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
	static PropertyInfo m_viewVisualTree = m_guiViewType.GetProperty("visualTree", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	static FieldInfo m_imguiContainerOnGui = typeof(IMGUIContainer).GetField("m_OnGUIHandler", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	static ScriptableObject m_currentToolbar;

	/// <summary>
	/// Callback for toolbar OnGUI method.
	/// </summary>
	public static Action OnToolbarGUI;

	static ToolbarCallback()
	{
		EditorApplication.update -= OnUpdate;
		EditorApplication.update += OnUpdate;
	}

	static void OnUpdate()
	{
		// Relying on the fact that toolbar is ScriptableObject and gets deleted when layout changes
		if (m_currentToolbar == null)
		{
			// Find toolbar
			var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
			m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;
			if (m_currentToolbar != null)
			{
				// Get it's visual tree
				var visualTree = (VisualElement) m_viewVisualTree.GetValue(m_currentToolbar, null);

				// Get first child which 'happens' to be toolbar IMGUIContainer
				var container = (IMGUIContainer) visualTree[0];

				// (Re)attach handler
				var handler = (Action) m_imguiContainerOnGui.GetValue(container);
				handler -= OnGUI;
				handler += OnGUI;
				m_imguiContainerOnGui.SetValue(container, handler);
			}
		}
	}

	static void OnGUI()
	{
		var handler = OnToolbarGUI;
		handler?.Invoke();
	}
}
