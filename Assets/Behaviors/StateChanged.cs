using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/StateChanged")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "StateChanged", message: "Agent changed state to [State]", category: "Events", id: "b0c7f234b014db7aaadbd37f47227242")]
public sealed partial class StateChanged : EventChannel<EnemyState> { }

