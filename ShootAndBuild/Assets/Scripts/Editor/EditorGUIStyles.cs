using UnityEngine;
using UnityEditor;

namespace SAB {

    //Class to hold custom gui styles
    public static class SABEditorGUIStyles
    {
        private static GUIStyle m_Line = null;

        //constructor
        static SABEditorGUIStyles()
        {
            m_Line = new GUIStyle("Box");
            m_Line.border.top = m_Line.border.bottom = 1;
            m_Line.margin.top = m_Line.margin.bottom = 1;
            m_Line.padding.top = m_Line.padding.bottom = 1;
        }

        public static GUIStyle EditorLine
        {
            get { return m_Line; }
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