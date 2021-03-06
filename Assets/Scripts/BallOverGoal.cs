﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class BallOverGoal : Photon.MonoBehaviour {

    GameObject goalBall;
	public ScorePoint.Player player;
	private Vector3 position;

	void Start(){
		position = new Vector3(0,3,0);
	}

	void OnCollisionEnter(Collision other)
    {	
        if(goalBall == null)
        {
            goalBall = MasterBallSyncManager.Instance.GetMasterGameBall();
        }

		if (goalBall == other.gameObject) { 
			Rigidbody rb =  goalBall.GetComponent<Rigidbody> ();
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			goalBall.transform.position = position;

			CountGoal ();
		}
	}

	void CountGoal(){
        
		if (player == ScorePoint.Player.Player_1) {
            photonView.RPC("IncreasePlayerPoints", PhotonTargets.All, "player1");
           
		} else {
            photonView.RPC("IncreasePlayerPoints", PhotonTargets.All, "player2");
        }
	}

    [PunRPC]
    void IncreasePlayerPoints(string  playerString)
    {
        ScoreCounter sc = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().GetScoreCounter();
        if (playerString.Equals("player1"))
        {
            sc.increasePointsPlayerOne();
        }
        else
        {
            sc.increasePointsPlayerTwo();
        }
    }
}