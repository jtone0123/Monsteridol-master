// RoomData.cs
// �� ���� �⺻ ������ ��ġ�� �ʿ��� ����(������)�� �����ϴ� ScriptableObject �Դϴ�.
using System.Collections.Generic;
using UnityEngine;


namespace RoomPlacementSystem
{
    [CreateAssetMenu(fileName = "NewRoomData", menuName = "MyGame/Room Data")]
    public class RoomData : ScriptableObject
    {
        [Header("�⺻ ����")]
        public string roomName = "�� ��";
        public Sprite roomIcon; // �Ǽ� �޴� UI�� ���� ������

        [Header("��ġ ����")]
        public GameObject roomPrefab; // ������ ���Կ� ��ġ�� ���� ���ӿ�����Ʈ ������

        [Header("������ ����")]
        public List<GameObject> GeneratedSchdules;
        public float scheduleTurnLate;
    }
}
