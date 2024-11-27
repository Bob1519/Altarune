using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageable : Damageable {

    [SerializeField] private int doubleDamageThreshold;
    [SerializeField] private float timeScaleShiftDuration;
    [SerializeField] private AnimationCurve timeScaleShiftCurve;

    protected override void BaseObject_OnTryDamage(int amount, ElementType element, EventResponse response) {
        if (!IFrameOn) {
            response.received = true;

            if (amount > 0) {
                int processedAmount = amount > doubleDamageThreshold ? 2 : 1;
                runtimeHP.DoDamage(processedAmount);
                baseObject.PropagateDamage(processedAmount);
                StartCoroutine(ISimulateIFrame());

                if (runtimeHP.Health <= 0) {
                    baseObject.Perish();
                    ToggleIFrame(true);
                    PHGameManager.Instance.DoGameOver();
                }
            }
        }
    }

    protected override IEnumerator ISimulateIFrame() {
        localIFrameOn = true;
        baseObject.ApplyMaterial(iFrameProperties.settings.flashMaterial);
        TimeScaleManager.Instance.AddTimeScaleShift(0, timeScaleShiftDuration,
                                                    timeScaleShiftCurve);
        yield return new WaitForSeconds(iFrameProperties.duration);
        baseObject.ResetMaterials();
        localIFrameOn = false;
    }
}