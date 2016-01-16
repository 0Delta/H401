using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;

public class GameEffect : MonoBehaviour {

    private enum _eParticleType {
        KIRAKIRA = 0,
        LAST_KIRAKIRA = 1,
        FLOWERS = 2,
    }

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

    private struct SmallerPoolInfo {
        public int   type;
        public int   id;
        public float time;
        public bool  isComplete;

        public SmallerPoolInfo(int _type, int _id, float _time = 0.0f, bool _isComplete = false) {
            type       = _type;
            id         = _id;
            time       = _time;
            isComplete = _isComplete;
        }
    };

    [SerializeField] private float effectPopPosZ = 0.0f;
    [SerializeField] private float effectMoveDurationTime = 0.0f;
    [SerializeField] private float effectMoveScaleUpTime = 0.0f;
    [SerializeField] private float effectMoveScaleDownTime = 0.0f;
    [SerializeField] private float effectMoveScaleUpSize = 0.0f;
    [SerializeField] private Color feverKirakiraColor;
    [SerializeField] private EffectParam kirakiraEffectParam;
    [SerializeField] private EffectParam lastKirakiraEffectParam;
    [SerializeField] private FlowerEffectParam[] flowerEffectParams;

    //ゲージの回復タイミングのために
    [HideInInspector]public Vector2 effectTimeInfo { get { return new Vector2(effectMoveDurationTime, effectMoveScaleUpTime + effectMoveScaleDownTime); } }
    
    private GameObject kirakiraController;
    private GameObject lastKirakiraController;
    private GameObject[] flowerControllers;
    private GameObject[] effectPrefabs;
    private ParticleSystem[] effectParticleSystems;
    private ParticlePool[][] effectPools;
    private int[] effectPoolSearchID;
    private Vector3 limitTimePos;
    private Vector3 feverGaugePos;

    private bool isSmaller;
    private List<SmallerPoolInfo> smallerPoolInfo;

    void Awake() {
        // エフェクト Prefab のメモリを確保
        flowerControllers = new GameObject[flowerEffectParams.Length];
        effectPrefabs = new GameObject[(int)_eParticleType.FLOWERS + flowerEffectParams.Length];
        effectParticleSystems = new ParticleSystem[(int)_eParticleType.FLOWERS + flowerEffectParams.Length];

        // Pool のメモリを確保
        effectPools = new ParticlePool[(int)_eParticleType.FLOWERS + flowerEffectParams.Length][];  // kirakiraEffect + lastKirakiraEffect + flowerEffect
        effectPools[(int)_eParticleType.KIRAKIRA] = new ParticlePool[kirakiraEffectParam.poolSize];
        effectPools[(int)_eParticleType.LAST_KIRAKIRA] = new ParticlePool[lastKirakiraEffectParam.poolSize];
        for(int i = 0; i < flowerEffectParams.Length; ++i) {
            effectPools[(int)_eParticleType.FLOWERS + i] = new ParticlePool[flowerEffectParams[i].param.poolSize];
        }

        // サーチIDのメモリを確保
        effectPoolSearchID = new int[(int)_eParticleType.FLOWERS + flowerEffectParams.Length];
        smallerPoolInfo = new List<SmallerPoolInfo>();
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
        effectPrefabs[(int)_eParticleType.KIRAKIRA] = Resources.Load<GameObject>(kirakiraEffectParam.prefabPath);
        effectPrefabs[(int)_eParticleType.LAST_KIRAKIRA] = Resources.Load<GameObject>(lastKirakiraEffectParam.prefabPath);
        for(int i = 0; i < flowerEffectParams.Length; ++i) {
            effectPrefabs[(int)_eParticleType.FLOWERS + i] = Resources.Load<GameObject>(flowerEffectParams[i].param.prefabPath);
        }

        // パーティクルの ParticleSystem を取得
        for(int i = 0; i < effectPrefabs.Length; ++i) {
            effectParticleSystems[i] = effectPrefabs[i].GetComponent<ParticleSystem>();
        }

        // ParticlePool を初期化
        int type = (int)_eParticleType.KIRAKIRA;
        for(int i = 0; i < kirakiraEffectParam.poolSize; ++i) {
            effectPools[type][i].obj = (GameObject)Instantiate(effectPrefabs[type], kirakiraController.transform.position, effectPrefabs[type].transform.rotation);
            effectPools[type][i].obj.transform.SetParent(kirakiraController.transform);
            effectPools[type][i].ps = effectPools[(int)_eParticleType.KIRAKIRA][i].obj.GetComponent<ParticleSystem>();
            effectPools[type][i].obj.SetActive(false);
            effectPools[type][i].isUse = false;
        }
        type = (int)_eParticleType.LAST_KIRAKIRA;
        for(int i = 0; i < lastKirakiraEffectParam.poolSize; ++i) {
            effectPools[type][i].obj = (GameObject)Instantiate(effectPrefabs[type], lastKirakiraController.transform.position, effectPrefabs[type].transform.rotation);
            effectPools[type][i].obj.transform.SetParent(lastKirakiraController.transform);
            effectPools[type][i].ps = effectPools[(int)_eParticleType.LAST_KIRAKIRA][i].obj.GetComponent<ParticleSystem>();
            effectPools[type][i].obj.SetActive(false);
            effectPools[type][i].isUse = false;
        }
        type = (int)_eParticleType.FLOWERS;
        for (int i = 0; i < flowerEffectParams.Length; ++i) {
            for(int j = 0; j < flowerEffectParams[i].param.poolSize; ++j) {
                effectPools[type + i][j].obj = (GameObject)Instantiate(effectPrefabs[type], flowerControllers[i].transform.position, effectPrefabs[type].transform.rotation);
                effectPools[type + i][j].obj.transform.SetParent(flowerControllers[i].transform);
                effectPools[type + i][j].ps = effectPools[(int)_eParticleType.FLOWERS + i][j].obj.GetComponent<ParticleSystem>();
                effectPools[type + i][j].obj.SetActive(false);
                effectPools[type + i][j].isUse = false;
            }
        }

        // poolSearchID を初期化
        for(int i = 0; i < (int)_eParticleType.FLOWERS + flowerEffectParams.Length; ++i)
            effectPoolSearchID[i] = 0;

        // 各種ゲージの position を取得
        GameInfoCanvas gic = gameSceneScript.gameUI.gameInfoCanvas;
        limitTimePos = gic.limitTime.transform.position;
        limitTimePos.z = effectPopPosZ;
        feverGaugePos = gic.feverGauge.transform.position;
        feverGaugePos.z = effectPopPosZ;

        // パーティクルが小さくなるストリームを生成
        isSmaller = false;
        Observable
            .EveryUpdate()
            .Where(_ => isSmaller)
            .Subscribe(_ => {
                for(int i = 0; i < smallerPoolInfo.Count; ++i) {
                    if(smallerPoolInfo[i].time > effectMoveDurationTime) {
                        float time = smallerPoolInfo[i].time - effectMoveDurationTime;
                        if(time > effectMoveScaleUpTime) {
                            time -= effectMoveScaleUpTime;
                            effectPools[smallerPoolInfo[i].type][smallerPoolInfo[i].id].ps.startSize = Mathf.Lerp(effectParticleSystems[smallerPoolInfo[i].type].startSize, 0.0f, time / effectMoveScaleDownTime);
                        } else {
                            effectPools[smallerPoolInfo[i].type][smallerPoolInfo[i].id].ps.startSize = Mathf.Lerp(effectParticleSystems[smallerPoolInfo[i].type].startSize, effectMoveScaleUpSize, time / effectMoveScaleUpTime);
                        }
                    }

                    SmallerPoolInfo spi = smallerPoolInfo[i];
                    spi.time += Time.deltaTime;
                    smallerPoolInfo[i] = spi;

                    if(smallerPoolInfo[i].time > effectMoveDurationTime + effectMoveScaleUpTime + effectMoveScaleDownTime) {
                        effectPools[smallerPoolInfo[i].type][smallerPoolInfo[i].id].ps.startSize = effectParticleSystems[smallerPoolInfo[i].type].startSize;
                        effectPools[smallerPoolInfo[i].type][smallerPoolInfo[i].id].obj.SetActive(false);
                        effectPools[smallerPoolInfo[i].type][smallerPoolInfo[i].id].isUse = false;

                        smallerPoolInfo.Remove(smallerPoolInfo[i]);
                        if(smallerPoolInfo.Count <= 0) {
                            isSmaller = false;
                        }
                    }
                }
            })
            .AddTo(gameObject);
    }

    // ツリー完成時の演出処理
    public void TreeCompleteEffect(List<Node> nodes, int score) {
        foreach(var node in nodes) {
            // エフェクト出現
            Vector3 pos = node.transform.position;
            pos.z = effectPopPosZ;

            int scoreRank = 0;
            for( ; scoreRank < flowerEffectParams.Length; ++scoreRank) {
                if(score >= flowerEffectParams[scoreRank].overScore) {
                    continue;
                }

                break;
            }

            // キラキラエフェクト
            if(scoreRank < flowerEffectParams.Length) {
                EffectSearchParticlePool(_eParticleType.KIRAKIRA, scoreRank, pos, _eParticleEffectType.MOVE_TIMEGAUGE);   // 体力ゲージ
                EffectSearchParticlePool(_eParticleType.KIRAKIRA, scoreRank, pos, _eParticleEffectType.MOVE_FEVERGAUGE);  // フィーバーゲージ
            } else {
                EffectSearchParticlePool(_eParticleType.LAST_KIRAKIRA, scoreRank, pos, _eParticleEffectType.MOVE_FEVERGAUGE);  // フィーバーゲージ
                EffectSearchParticlePool(_eParticleType.LAST_KIRAKIRA, scoreRank, pos, _eParticleEffectType.MOVE_TIMEGAUGE);   // 体力ゲージ
            }

            // 枝先か壁なら花エフェクトを出現
            if((node.Temp.LinkNum == 1 || node.Temp.LinkNum >= 3 || node.CheckLinkedWall()) && node.NodeID.y >= 2) {
                if(scoreRank >= flowerEffectParams.Length)
                    --scoreRank;

                EffectSearchParticlePool(_eParticleType.FLOWERS, scoreRank, pos);
            }
        }
    }

    void EffectSearchParticlePool(_eParticleType particleType, int scoreRank, Vector3 appearPos, _eParticleEffectType pet = _eParticleEffectType.NONE) {
        int type = (int)particleType;
        if(particleType == _eParticleType.FLOWERS)
            type += scoreRank;

        for(int i = 0; i < effectPools[type].Length; ++i) {
            if (effectPools[type][effectPoolSearchID[type]].isUse != true) {
                effectPools[type][effectPoolSearchID[type]].obj.transform.position = appearPos;

                // キラキラの色をスコアに合わせて変更
                if(particleType == _eParticleType.KIRAKIRA) {
                    if(pet != _eParticleEffectType.MOVE_FEVERGAUGE) {
                        if(scoreRank > 0)
                            effectPools[type][effectPoolSearchID[type]].ps.startColor = flowerEffectParams[scoreRank - 1].kirakiraColor;
                    } else {
                        effectPools[type][effectPoolSearchID[type]].ps.startColor = feverKirakiraColor;
                    }
                }

                // エフェクトを有効化
                effectPools[type][effectPoolSearchID[type]].obj.SetActive(true);
                effectPools[type][effectPoolSearchID[type]].isUse = true;

                switch(pet) {
                    case _eParticleEffectType.MOVE_TIMEGAUGE:
                        MoveFinishedBanish(type, limitTimePos, effectMoveDurationTime);
                        break;

                    case _eParticleEffectType.MOVE_FEVERGAUGE:
                        MoveFinishedBanish(type, feverGaugePos, effectMoveDurationTime);
                        break;

                    default:
                        break;
                }

                ++effectPoolSearchID[type];
                if(effectPoolSearchID[type] >= effectPools[type].Length)
                    effectPoolSearchID[type] = 0;

                break;
            }

            ++effectPoolSearchID[type];
            if(effectPoolSearchID[type] >= effectPools[type].Length)
                effectPoolSearchID[type] = 0;
        }
    }
    
    void MoveFinishedBanish(int type, Vector3 endPos, float duration) {
        isSmaller = true;
        smallerPoolInfo.Add(new SmallerPoolInfo(type, effectPoolSearchID[type]));
        
        effectPools[type][effectPoolSearchID[type]].obj.transform.DOMove(endPos, duration)
            .SetEase(Ease.InCubic);
    }
}
