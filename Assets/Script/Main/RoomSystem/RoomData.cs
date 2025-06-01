// RoomData.cs
// �� ���� �⺻ ������ ��ġ�� �ʿ��� ����(������)�� �����ϴ� ScriptableObject �Դϴ�.
using UnityEngine;

namespace RoomPlacementSystem
{
    [CreateAssetMenu(fileName = "NewRoomData", menuName = "Game/Room Data")]
    public class RoomData : ScriptableObject
    {
        [Header("�⺻ ����")]
        public string roomName = "�� ��";
        public Sprite roomIcon; // �Ǽ� �޴� UI�� ���� ������

        [Header("��ġ ����")]
        public GameObject roomPrefab; // ������ ���Կ� ��ġ�� ���� ���ӿ�����Ʈ ������
    }
}
