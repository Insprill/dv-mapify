using CommsRadioAPI;
using DV;

namespace Mapify.CarLabeler
{
    public abstract class SubState: AStateBehaviour
    {
        public const string LABELER_TITLE = "car labeler";

        protected YardDestination destination;

        protected SubState(YardDestination aDestination, string contentText_) :
            base(new CommsRadioState(
                titleText: LABELER_TITLE,
                buttonBehaviour: ButtonBehaviourType.Override,
                contentText: contentText_
            ))
        {
            destination = aDestination;
        }
    }
}
