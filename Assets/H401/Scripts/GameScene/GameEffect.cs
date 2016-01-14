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
        public ParticleSystem ps;
        public bool           isUse;
    }

    [System.Serializable] private struct EffectParam {
        public string prefabPath;
        public string controllerName;
        public int    poolSize;
    };
    
    [System.Serializable] private struct FlowerEffectParam {
        public EffectParam param;
        public int         overScore;
        public Color       kirakiraColor;
    };

    [SerializeField] private float effectPopPosZ = 0.0f;
    [SerializeField] private float effectMoveDurationTime = 0.0f;
    [SerializeField] private float effectMoveWaitTime = 0.0f;
    [SerializeField] private Color feverKirakiraColor;
    [SerializeField] private EffectParam kirakiraEffectParam;
    [SerializeField] private EffectParam lastKirakiraEffectParam;
    [SerializeField] private FlowerEffectParam[] flowerEffectParams;

    //ゲージの回復タイミングのために
    [HideInInspector]public Vector2 effectTimeInfo { get { return new Vector2(effectMoveDurationTime,effectMoveWaitTime); } }
    
    private GameObject kirakiraController;
    private GameObject lastKirakiraController;
    private GameObject[] flowerControllers;
    private GameObject kirakiraPrefab;
    private GameObject lastKirakiraPrefab;
    private GameObject[] flowerPrefabs;
    private ParticlePool[] kirakiraPool;
    private ParticlePool[] lastKirakiraPool;
    private ParticlePool[][] flowerPools;
    private int kirakiraPoolSearchID;
    private int lastKirakiraPoolSearchID;
    private int[] flowerPoolSearchID;
    private Vector3 limitTimePos;
    private Vector3 feverGaugePos;

    void Awake() {
        // 花エフェクト関連のメモリを確保
        flowerControllers = new GameObject[flowerEffectParams.Length];
        flowerPrefabs = new GameObject[flowerEffectParams.Length];

        // Pool のメモリを確保
        kirakiraPool = new ParticlePool[kirakiraEffectParam.poolSize];
        lastKirakiraPool = new ParticlePool[lastKirakiraEffectParam.poolSize];
        flowerPools = new ParticlePool[flowerEffectParams.Length][];
        for(int i = 0; i < flowerEffectParams.Length; ++i) {
            flowerPools[i] = new ParticlePool[flowerEffectParams[i].param.poolSize];
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
        lastKirakiraController = new GameObject();
        lastKirakiraController.transform.SetParent(transform.parent);
        lastKirakiraController.name = lastKirakiraEffectParam.controllerName;
        for(int i = 0; i < flowerEffectParams.Length; ++i) {
            flowerControllers[i] = new GameObject();
            flowerControllers[i].transform.SetParent(transform.parent);
            flowerControllers[i].name = flowerEffectParams[i].param.controllerName;
        }

        // パーティクルの prefab を取得
        kirakiraPrefab = Resources.Load<GameObject>(kirakiraEffectParam.prefabPath);
        lastKirakiraPrefab = Resources.Load<GameObject>(lastKirakiraEffectParam.prefabPath);
        for(int i = 0; i < flowerEffectParams.Length; ++i) {
            flowerPrefabs[i] = Resources.Load<GameObject>(flowerEffectParams[i].param.prefabPath);
        }

        // ParticlePool を初期化
        for(int i = 0; i < kirakiraEffectParam.poolSize; ++i) {
            kirakiraPool[i].obj = (GameObject)Instantiate(kirakiraPrefab, kirakiraController.transform.position, kirakiraPrefab.transform.rotation);
            kirakiraPool[i].obj.transform.SetParent(kirakiraController.transform);
            kirakiraPool[i].ps = kirakiraPool[i].obj.GetComponent<ParticleSystem>();
            kirakiraPool[i].obj.SetActive(false);
            kirakiraPool[i].isUse = false;
        }
        for(int i = 0; i < lastKirakiraEffectParam.poolSize; ++i) {
            lastKirakiraPool[i].obj = (GameObject)Instantiate(lastKirakiraPrefab, lastKirakiraController.transform.position, lastKirakiraPrefab.transform.rotation);
            lastKirakiraPool[i].obj.transform.SetParent(lastKirakiraController.transform);
            lastKirakiraPool[i].ps = lastKirakiraPool[i].obj.GetComponent<ParticleSystem>();
            lastKirakiraPool[i].obj.SetActive(false);
            lastKirakiraPool[i].isUse = false;
        }
        for (int i = 0; i < flowerEffectParams.Length; ++i) {
            for(int j = 0; j < flowerEffectParams[i].param.poolSize; ++j) {
                flowerPools[i][j].obj = (GameObject)Instantiate(flowerPrefabs[i], flowerControllers[i].transform.position, flowerPrefabs[i].transform.rotation);
                flowerPools[i][j].obj.transform.SetParent(flowerControllers[i].transform);
                flowerPools[i][j].ps = flowerPools[i][j].obj.GetComponent<ParticleSystem>();
                flowerPools[i][j].obj.SetActive(false);
                flowerPools[i][j].isUse = false;
            }
        }

        // poolSearchID を初期化
        kirakiraPoolSearchID = 0;
        lastKirakiraPoolSearchID = 0;
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
    public void TreeCompleteEffect(List<Node> nodes, int score) {
        foreach(var node in nodes) {
            // エフェクト出現
            Vector3 pos = node.transform.position;
            pos.z = effectPopPosZ;

            int i = 0;
            for( ; i < flowerEffectParams.Length; ++i) {
                if(score >= flowerEffectParams[i].overScore) {
                    continue;
                }

                break;
            }

            // キラキラエフェクト
            if(i < flowerEffectParams.Length) {
                KirakiraSearchParticlePool(i, pos, _eParticleEffectType.MOVE_TIMEGAUGE);   // 体力ゲージ
                KirakiraSearchParticlePool(i, pos, _eParticleEffectType.MOVE_FEVERGAUGE);  // フィーバーゲージ
            } else {
                LastKirakiraSearchParticlePool(pos, _eParticleEffectType.MOVE_TIMEGAUGE);   // 体力ゲージ
                LastKirakiraSearchParticlePool(pos, _eParticleEffectType.MOVE_FEVERGAUGE);  // フィーバーゲージ
            }

            // 枝先か壁なら花エフェクトを出現
            if((node.Temp.LinkNum == 1 || node.Temp.LinkNum >= 3 || node.CheckLinkedWall()) && node.NodeID.y >= 2) {
                if(i > 0)
                    FlowerSearchParticlePool(pos, i - 1);
            }
        }
    }

    void KirakiraSearchParticlePool(int colorType, Vector3 appearPos, _eParticleEffectType pet = _eParticleEffectType.NONE) {
        for(int i = 0; i < kirakiraEffectParam.poolSize; ++i) {
            if (kirakiraPool[kirakiraPoolSearchID].isUse != true) {
                kirakiraPool[kirakiraPoolSearchID].obj.transform.position = appearPos;

                // キラキラの色をスコアに合わせて変更
                if(pet != _eParticleEffectType.MOVE_FEVERGAUGE) {
                    if(colorType > 0) {
                        kirakiraPool[kirakiraPoolSearchID].ps.startColor = flowerEffectParams[colorType - 1].kirakiraColor;
                    }
                } else {
                    kirakiraPool[kirakiraPoolSearchID].ps.startColor = feverKirakiraColor;
                }

                // キラキラエフェクトを有効化
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

    void LastKirakiraSearchParticlePool(Vector3 appearPos, _eParticleEffectType pet = _eParticleEffectType.NONE) {
        for(int i = 0; i < lastKirakiraEffectParam.poolSize; ++i) {
            if (lastKirakiraPool[lastKirakiraPoolSearchID].isUse != true) {
                lastKirakiraPool[lastKirakiraPoolSearchID].obj.transform.position = appearPos;
                
                // キラキラエフェクトを有効化
                lastKirakiraPool[lastKirakiraPoolSearchID].obj.SetActive(true);
                lastKirakiraPool[lastKirakiraPoolSearchID].isUse = true;

                switch(pet) {
                    case _eParticleEffectType.MOVE_TIMEGAUGE:
                        MoveFinishedBanish(lastKirakiraPool, lastKirakiraPoolSearchID, limitTimePos, effectMoveDurationTime, effectMoveWaitTime);
                        break;

                    case _eParticleEffectType.MOVE_FEVERGAUGE:
                        MoveFinishedBanish(lastKirakiraPool, lastKirakiraPoolSearchID, feverGaugePos, effectMoveDurationTime, effectMoveWaitTime);
                        break;

                    default:
                        break;
                }

                ++lastKirakiraPoolSearchID;
                if(lastKirakiraPoolSearchID >= lastKirakiraEffectParam.poolSize)
                    lastKirakiraPoolSearchID = 0;

                break;
            }
        }
    }

    void FlowerSearchParticlePool(Vector3 appearPos, int type, _eParticleEffectType pet = _eParticleEffectType.NONE) {
        for(int i = 0; i < flowerEffectParams[type].param.poolSize; ++i) {
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
                if(flowerPoolSearchID[type] >= flowerEffectParams[type].param.poolSize)
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
