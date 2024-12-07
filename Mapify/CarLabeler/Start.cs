using CommsRadioAPI;

namespace Mapify.CarLabeler
{
    public class Start: AStateBehaviour
    {
        public Start() : base(new CommsRadioState(
            titleText: SubState.LABELER_TITLE,
            contentText: "enable yard car labeler?"
            ))
        {}

        public override AStateBehaviour OnAction(CommsRadioUtility utility, InputAction action)
        {
            switch (action)
            {
                case InputAction.Activate:
                    utility.PlaySound(VanillaSoundCommsRadio.ModeEnter);
                    return new SelectStation(SetupDestination());
                default:
                    return new Start();
            }
        }

        private static YardDestination SetupDestination()
        {
            var aDestination = Mapify.Settings.lastUsedLabel;
            YardDestination.Validate(ref aDestination);
            return aDestination;
        }
    }
}
