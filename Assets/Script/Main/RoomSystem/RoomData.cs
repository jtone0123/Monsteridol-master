// RoomData.cs
// 각 방의 기본 정보와 배치에 필요한 에셋(프리팹)을 정의하는 ScriptableObject 입니다.
using System.Collections.Generic;
using UnityEngine;


namespace RoomPlacementSystem
{
    [CreateAssetMenu(fileName = "NewRoomData", menuName = "MyGame/Room Data")]
    public class RoomData : ScriptableObject
    {
        [Header("기본 정보")]
        public string roomName = "새 방";
        public Sprite roomIcon; // 건설 메뉴 UI에 사용될 아이콘

        [Header("배치 관련")]
        public GameObject roomPrefab; // 실제로 슬롯에 배치될 방의 게임오브젝트 프리팹

        [Header("스케줄 관련")]
        public List<GameObject> GeneratedSchdules;
        public float scheduleTurnLate;
    }
}
