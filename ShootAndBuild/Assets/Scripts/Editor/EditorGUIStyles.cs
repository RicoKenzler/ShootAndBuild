using UnityEngine;
using UnityEditor;

namespace SAB {

    //Class to hold custom gui styles
    public static class SABEditorGUIStyles
    {
        private static GUIStyle m_line = null;

        //constructor
        static SABEditorGUIStyles()
        {
            m_line = new GUIStyle("Box");
            m_line.border.top = m_line.border.bottom = 1;
            m_line.margin.top = m_line.margin.bottom = 1;
            m_line.padding.top = m_line.padding.bottom = 1;
        }

        public static GUIStyle EditorLine
        {
            get { return m_line; }
        }
    }


    public static class SABEditorGUI
    {
        public static void Seperator()
        {
            GUILayout.Box(GUIContent.none, SABEditorGUIStyles.EditorLine, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
        }
    }
}