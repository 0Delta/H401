using UnityEngine;
using System.Collections;

public class LevelController : MonoBehaviour {

    [SerializeField]private float DeviceAngle;

    public GameObject gameController = null; 

	// Use this for initialization
	void Start () {
        Input.gyro.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {

        //姿勢が45度以上135度以下
        if(Input.gyro.attitude.eulerAngles.y > 45 && Input.gyro.attitude.eulerAngles.y < 135)
        {

            
        }

        //if
	}

    //難易度切り替え状態へ
    public void FieldChangeStart()
    {
            //ゲームをノンアクに
            gameController.SetActive(false);

            //難易度選択をinstantiateする
    }
}
