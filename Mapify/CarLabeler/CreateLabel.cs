using CommsRadioAPI;
using DV;
using UnityEngine;

namespace Mapify.CarLabeler
{
    public class CreateLabel: SubState
    {
        private TrainCar pointedCar;
        private RaycastHit[] hits = new RaycastHit[5];

        private static bool isSetup;
        private static GameObject trainHighlighter;
        private static Material selectionMaterial;
        private static MeshRenderer trainHighlighterRender;

        public CreateLabel(YardDestination aDestination) :
            base(aDestination,
                $"Point at a car and click to label it.\n" +
                    $"{aDestination.StationName()}-{aDestination.YardID}-{aDestination.TrackNumber}"
            )
        {
            if(isSetup) return;
            DoSetup();
        }

        private static void DoSetup()
        {
            var deleter = (CommsRadioCarDeleter)ControllerAPI.GetVanillaMode(VanillaMode.Clear);

            if (deleter == null)
            {
                Mapify.LogError($"{nameof(CreateLabel)}: Could not get deleter");
                return;
            }

            trainHighlighter = deleter.trainHighlighter;
            selectionMaterial = deleter.selectionMaterial;
            trainHighlighterRender = deleter.trainHighlighterRender;

            isSetup = true;
        }

        public override AStateBehaviour OnAction(CommsRadioUtility utility, InputAction action)
        {
            switch (action)
            {
                case InputAction.Activate:
                    MarkCar(utility);
                    goto default; //C#, why...
                default:
                    return new CreateLabel(destination);
            }
        }

        private void MarkCar(CommsRadioUtility utility)
        {
            if (pointedCar == null){
                utility.PlaySound(VanillaSoundCommsRadio.Warning);
                return;
            }

            var labelComponent = pointedCar.gameObject.GetComponent<YardDestinationComponent>();
            if (labelComponent == null)
            {
                labelComponent = pointedCar.gameObject.AddComponent<YardDestinationComponent>();
            }

            labelComponent.Setup(destination);
            utility.PlaySound(VanillaSoundCommsRadio.Confirm);
        }

        public override AStateBehaviour OnUpdate(CommsRadioUtility utility)
        {
            TrainCar car = null;

            // Why does the direction need to be downward? I would expect forward.
            if(Physics.RaycastNonAlloc(utility.SignalOrigin.position, -utility.SignalOrigin.up, hits, maxDistance: 100f) > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.transform == null) continue;

                    car = TrainCar.Resolve(hit.transform.root);
                    if (car == null) continue;

                    Mapify.LogDebug($"hit car {car.gameObject.name}");
                    Mapify.LogDebug($"layer: {car.gameObject.layer}");
                    break;
                }
            }

            PointToCar(car, utility);
            return this;
        }

        private void PointToCar(TrainCar car, CommsRadioUtility utility)
        {
            if (pointedCar == car) return;

            pointedCar = car;
            ClearHighlightCar();

            if (pointedCar == null) return;

            HighlightCar(pointedCar);
            utility.PlaySound(VanillaSoundCommsRadio.HoverOver);
        }

        private static void HighlightCar(TrainCar car)
        {
            Mapify.LogDebug($"{nameof(HighlightCar)}: {car}");

            trainHighlighterRender.material = selectionMaterial;
            var transform = trainHighlighter.transform;
            var bounds = car.Bounds;
            var vector3_1 = bounds.size + CommsRadioCarDeleter.HIGHLIGHT_BOUNDS_EXTENSION;
            transform.localScale = vector3_1;
            var vector3_2 = car.transform.up * (trainHighlighter.transform.localScale.y / 2f);
            var forward = car.transform.forward;
            bounds = car.Bounds;
            double z = bounds.center.z;
            var vector3_3 = forward * (float) z;
            trainHighlighter.transform.SetPositionAndRotation(car.transform.position + vector3_2 + vector3_3, car.transform.rotation);
            trainHighlighter.SetActive(true);
            trainHighlighter.transform.SetParent(car.transform, true);
        }

        private static void ClearHighlightCar()
        {
            Mapify.LogDebug($"{nameof(ClearHighlightCar)}");

            trainHighlighter.SetActive(false);
            trainHighlighter.transform.SetParent(null);
        }
    }
}
