using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HumanoidTracer
{
    /// <summary>
    /// ���݊֐ߊp�x�擾�̃e�X�g�p
    /// </summary>
    public class AngleMonitor : MonoBehaviour
    {
        private ArticulationBody[] bodies;

        // Start is called before the first frame update
        void Start()
        {
            // ���̃I�u�W�F�N�g�ȉ��� ArticulationBody ���܂Ƃ߂Ď擾
            bodies = gameObject.GetComponentsInChildren<ArticulationBody>(true);
        }

        // ��]�֐߂̊p�x��\��
        private void OnGUI()
        {
            string text = "";
            foreach (var body in bodies)
            {
                // ��]�֐߂̂ݑΏۂƂ���i�ꉞ�A1���R�x�����`�F�b�N�j
                if ((body.jointType == ArticulationJointType.RevoluteJoint) && (body.dofCount == 1))
                {
                    // ���O�ƌ��݊p�x[deg]��\��������ɒǉ�
                    float angle = body.jointPosition[0] * Mathf.Rad2Deg;
                    text += $"{body.name}: {angle:#.#}\n";
                }
            }

            // �\��
            GUI.TextArea(new Rect(10, 10, 200, 400), text);
        }
    }
}
