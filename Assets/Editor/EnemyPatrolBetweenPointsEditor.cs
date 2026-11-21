using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyPatrolBetweenPoints))]
public class EnemyPatrolBetweenPointsEditor : Editor
{
    private SerializedProperty patrolRangeProperty;

    private void OnEnable()
    {
        patrolRangeProperty = serializedObject.FindProperty("patrolRange");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(patrolRangeProperty, new GUIContent("Rango ± desde la posición inicial"));

        Vector2 range = patrolRangeProperty.vector2Value;
        range.x = Mathf.Min(range.x, -0.25f);
        range.y = Mathf.Max(range.y, 0.25f);
        patrolRangeProperty.vector2Value = range;

        DrawPropertiesExcluding(serializedObject, "m_Script", "patrolRange");
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        EnemyPatrolBetweenPoints patrol = (EnemyPatrolBetweenPoints)target;
        Transform transform = patrol.transform;
        Vector2 range = patrolRangeProperty.vector2Value;

        float leftX = transform.position.x + Mathf.Min(range.x, -0.25f);
        float rightX = transform.position.x + Mathf.Max(range.y, 0.25f);

        Handles.color = new Color(1f, 0.85f, 0f, 0.95f);
        Vector3 leftPosition = new Vector3(leftX, transform.position.y, 0f);
        Vector3 rightPosition = new Vector3(rightX, transform.position.y, 0f);

        EditorGUI.BeginChangeCheck();
        Vector3 newLeft = Handles.FreeMoveHandle(
            leftPosition,
            0.16f,
            Vector3.zero,
            Handles.SphereHandleCap);
        Vector3 newRight = Handles.FreeMoveHandle(
            rightPosition,
            0.16f,
            Vector3.zero,
            Handles.SphereHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(patrol, "Cambiar rango de patrulla");

            float leftOffset = Mathf.Min(newLeft.x - transform.position.x, -0.25f);
            float rightOffset = Mathf.Max(newRight.x - transform.position.x, 0.25f);

            patrolRangeProperty.vector2Value = new Vector2(leftOffset, rightOffset);
            serializedObject.ApplyModifiedProperties();
        }

        Handles.DrawAAPolyLine(5f, leftPosition, rightPosition);
        Handles.Label(leftPosition + Vector3.up * 0.35f, "Izquierda");
        Handles.Label(rightPosition + Vector3.up * 0.35f, "Derecha");
        Handles.Label(transform.position + Vector3.up * 0.35f, "Inicio");
    }
}
