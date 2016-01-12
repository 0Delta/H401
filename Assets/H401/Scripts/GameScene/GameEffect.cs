using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GameEffect : MonoBehaviour {

    private enum _eParticleEffectType {
        MOVE_TIMEGAUGE,
        MOVE_FEVERGAUGE,

        NONE,
        MAX
    }

    private struct ParticlePool {
        public GameObject     obj;
        public bool           isUse;
    }

    [System.Serializable] private struct EffectParam {
        public string prefabPath;
        public string controllerName;
        public int overScore;
        public int poolSize;
    };

    [SerializeField] private float effectPopPosZ = 0.0f;
    [SerializeField] private float effectMoveDurationTime = 0.0f;
    [SerializeField] private float effectMoveWaitTime = 0.0f;
    [SerializeField] private EffectParam   kirakiraEffectParam;
    [SerializeField] private EffectParam[] flowerEffectParams;
    
    private GameObject kirakiraController;
    private GameObject[] flowerControllers;
    private GameObject kirakiraPrefab;
    private GameObject[] flowerPrefabs;
    private ParticlePool[] kirakiraPool;
    private ParticlePool[][] flowerPools;
    private int kirakiraPoolSearchID;
    private int[] flowerPoolSearchID;
    private Vector3 limitTimePos;
    private Vector3 feverGaugePos;

    void Awake() {
        // 花エフェクト関連のメモリを確保
        flowerControllers = new GameObject[flowerEffectParams.Length];
        flowerPrefabs = new GameObject[flowerEffectParams.Length];

        // Pool のメモリを確保
        kirakiraPool = new ParticlePool[kirakiraEffectParam.poolSize];
        flowerPools = new ParticlePool[flowerEffectParams.Length][];
        for(int i = 0; i < flowerEffectParams.Length; ++i) {
            flowerPools[i] = new ParticlePool[flowerEffectParams[i].poolSize];
        }

        // サーチIDのメモリを確保
        flowerPoolSearchID = new int[flowerEffectParams.Length];
    }

    void Start() {
        // ゲームシーンを取得
        GameObject gameScene = transform.root.GetComponent<AppliController>().GetCurrentScene();
        GameScene gameSceneScript = gameScene.GetComponent<GameScene>();

        // パーティクルコントローラーを生成
        kirakiraController = new GameObject();
        kirakiraController.transform.SetParent(transform.parent);
        kirakiraController.name = kirakiraEffectParam.controllerName;
        for(int i = 0; i < flowerEffectParams.Length; ++i) {
            flowerControllers[i] = new GameObject();
            flowerControllers[i].transform.SetParent(transform.parent);
            flowerControllers[i].name = flowerEffectParams[i].controllerName;
        }

        // パーティクルの prefab を取得
        kirakiraPrefab = Resources.Load<GameObject>(kirakiraEffectParam.prefabPath);
        for(int i = 0; i < flowerEffectParams.Length; ++i) {
            flowerPrefabs[i] = Resources.Load<GameObject>(flowerEffectParams[i].prefabPath);
        }

        // ParticlePool を初期化
        for(int i = 0; i < kirakiraEffectParam.poolSize; ++i) {
            kirakiraPool[i].obj = (GameObject)Instantiate(kirakiraPrefab, kirakiraController.transform.position, kirakiraController.transform.rotation);
            kirakiraPool[i].obj.transform.SetParent(kirakiraController.transform);
            kirakiraPool[i].obj.SetActive(false);
            kirakiraPool[i].isUse = false;
        }
        for (int i = 0; i < flowerEffectParams.Length; ++i) {
            for(int j = 0; j < flowerEffectParams[i].poolSize; ++j) {
                flowerPools[i][j].obj = (GameObject)Instantiate(flowerPrefabs[i], flowerControllers[i].transform.position, flowerControllers[i].transform.rotation);
                flowerPools[i][j].obj.transform.SetParent(flowerControllers[i].transform);
                flowerPools[i][j].obj.SetActive(false);
                flowerPools[i][j].isUse = false;
            }
        }

        // poolSearchID を初期化
        kirakiraPoolSearchID = 0;
        for(int i = 0; i < flowerEffectParams.Length; ++i)
            flowerPoolSearchID[i] = 0;

        // 各種ゲージの position を取得
        GameInfoCanvas gic = gameSceneScript.gameUI.gameInfoCanvas;
        limitTimePos = gic.limitTime.transform.position;
        limitTimePos.z = effectPopPosZ;
        feverGaugePos = gic.feverGauge.transform.position;
        feverGaugePos.z = effectPopPosZ;
    }

    // ツリー完成時の演出処理
    public void TreeCompleteEffect(List<GameObject> nodes, int score) {
        foreach(var node in nodes) {
            // エフェクト出現
            Vector3 pos = node.transform.position;
            pos.z = effectPopPosZ;

            // キラキラエフェクト
            KirakiraSearchParticlePool(pos, _eParticleEffectType.MOVE_TIMEGAUGE);   // 体力ゲージ
            KirakiraSearchParticlePool(pos, _eParticleEffectType.MOVE_FEVERGAUGE);  // フィーバーゲージ
            
            Node nodeScript = node.GetComponent<Node>();
            // 枝先か壁ならエフェクトを出現
            if(nodeScript.Temp.LinkNum == 1 || nodeScript.Temp.LinkNum >= 3 || nodeScript.CheckLinkedWall()) {
                int i = 0;
                for( ; i < flowerEffectParams.Length; ++i) {
                    if(score >= flowerEffectParams[i].overScore) {
                        continue;
                    }

                    break;
                }
                --i;

                // 花エフェクト
                if(i >= 0)
                    FlowerSearchParticlePool(pos, i);
            }
        }
    }

    void KirakiraSearchParticlePool(Vector3 appearPos, _eParticleEffectType pet = _eParticleEffectType.NONE) {
        for(int i = 0; i < kirakiraEffectParam.poolSize; ++i) {
            if (kirakiraPool[kirakiraPoolSearchID].isUse != true) {
                kirakiraPool[kirakiraPoolSearchID].obj.transform.position = appearPos;
                kirakiraPool[kirakiraPoolSearchID].obj.SetActive(true);
                kirakiraPool[kirakiraPoolSearchID].isUse = true;

                switch(pet) {
                    case _eParticleEffectType.MOVE_TIMEGAUGE:
                        MoveFinishedBanish(kirakiraPool, kirakiraPoolSearchID, limitTimePos, effectMoveDurationTime, effectMoveWaitTime);
                        break;

                    case _eParticleEffectType.MOVE_FEVERGAUGE:
                        MoveFinishedBanish(kirakiraPool, kirakiraPoolSearchID, feverGaugePos, effectMoveDurationTime, effectMoveWaitTime);
                        break;

                    default:
                        break;
                }

                ++kirakiraPoolSearchID;
                if(kirakiraPoolSearchID >= kirakiraEffectParam.poolSize)
                    kirakiraPoolSearchID = 0;

                break;
            }
        }
    }

    void FlowerSearchParticlePool(Vector3 appearPos, int type, _eParticleEffectType pet = _eParticleEffectType.NONE) {
        for(int i = 0; i < flowerEffectParams[type].poolSize; ++i) {
            if (flowerPools[type][flowerPoolSearchID[type]].isUse != true) {
                flowerPools[type][flowerPoolSearchID[type]].obj.transform.position = appearPos;
                flowerPools[type][flowerPoolSearchID[type]].obj.SetActive(true);
                flowerPools[type][flowerPoolSearchID[type]].isUse = true;

                switch(pet) {
                    case _eParticleEffectType.MOVE_TIMEGAUGE:
                        MoveFinishedBanish(flowerPools[type], flowerPoolSearchID[type], limitTimePos, effectMoveDurationTime, effectMoveWaitTime);
                        break;

                    case _eParticleEffectType.MOVE_FEVERGAUGE:
                        MoveFinishedBanish(flowerPools[type], flowerPoolSearchID[type], feverGaugePos, effectMoveDurationTime, effectMoveWaitTime);
                        break;

                    default:
                        break;
                }

                ++flowerPoolSearchID[type];
                if(flowerPoolSearchID[type] >= flowerEffectParams[type].poolSize)
                    flowerPoolSearchID[type] = 0;

                break;
            }
        }
    }
    
    void MoveFinishedBanish(ParticlePool[] pool, int poolID, Vector3 endPos, float duration, float waitTime) {
        pool[poolID].obj.transform.DOMove(endPos, duration)
            .OnComplete(() => {
                pool[poolID].obj.transform.DOScale(0.0f, waitTime)
                .OnComplete(() => {
                    pool[poolID].obj.SetActive(false);
                    pool[poolID].isUse = false;
                });
            })
            .SetEase(Ease.InCubic);
    }
}
