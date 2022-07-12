using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HumanoidTracer
{
    /// <summary>
    /// 現在関節角度取得のテスト用
    /// </summary>
    public class AngleMonitor : MonoBehaviour
    {
        private ArticulationBody[] bodies;

        // Start is called before the first frame update
        void Start()
        {
            // このオブジェクト以下の ArticulationBody をまとめて取得
            bodies = gameObject.GetComponentsInChildren<ArticulationBody>(true);
        }

        // 回転関節の角度を表示
        private void OnGUI()
        {
            string text = "";
            foreach (var body in bodies)
            {
                // 回転関節のみ対象とする（一応、1自由度かもチェック）
                if ((body.jointType == ArticulationJointType.RevoluteJoint) && (body.dofCount == 1))
                {
                    // 名前と現在角度[deg]を表示文字列に追加
                    float angle = body.jointPosition[0] * Mathf.Rad2Deg;
                    text += $"{body.name}: {angle:#.#}\n";
                }
            }

            // 表示
            GUI.TextArea(new Rect(10, 10, 200, 400), text);
        }
    }
}
