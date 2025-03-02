using UnityEngine.Events;

namespace Runtime.Ads {
    public interface IShowAdCondition {
        event UnityAction OnConditionMet;
    }
}
