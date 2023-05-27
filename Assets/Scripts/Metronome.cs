using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(AudioSource))]
public class Metronome : MonoBehaviour {
    
    public static Metronome INSTANCE { get; private set; }

    [Header("Song Settings")]
    [FormerlySerializedAs("src")] [SerializeField] private AudioSource music;
    [SerializeField] private float bpm;
    [SerializeField] private float offset;

    [Header("Accuracy Settings")] 
    [SerializeField] private float accuracy;
    //Have a custom property drawer for configuring accuracy ranges and move them out of hard floats
    [SerializeField] private float missWindow;
    [SerializeField] private float fiftyWindow;
    [SerializeField] private float oneHundredWindow;
    [SerializeField] private float threeHundredWindow;

    [Header("On Beat stuff")] 
    [SerializeField] public UnityEvent OnBeat;
    

    //Internal metronome information
    private float dspStartTime;
    private float secondsPerBeat;
    private float nextBeatPosition;

    public float songPosition { get; private set; }
    public bool isRunning { get; private set; }

    private void Awake() {
        INSTANCE = this;
    }

    public void StartMetronome() {
        music.Play();
        isRunning = true;
        dspStartTime = (float)AudioSettings.dspTime;
        secondsPerBeat = 60f / bpm;
        nextBeatPosition = 0;
    }

    // Update is called once per frame
    void Update() {
        if (!isRunning) return;
        songPosition = (float)(AudioSettings.dspTime - dspStartTime) - offset;
        if (songPosition >= nextBeatPosition) {
            OnBeat.Invoke();
            nextBeatPosition += secondsPerBeat;
        }
    }

    public Accuracy GetAccuracy() {
        float timeInSecondsToNextBeat = nextBeatPosition - songPosition;
        float timeInSecondsToLastBeat = songPosition - (nextBeatPosition - secondsPerBeat);
        if (timeInSecondsToNextBeat < threeHundredWindow || timeInSecondsToLastBeat < threeHundredWindow) return Accuracy.THREE_HUNDRED;
        if (timeInSecondsToNextBeat < oneHundredWindow || timeInSecondsToLastBeat < threeHundredWindow) return Accuracy.ONE_HUNDRED;
        if (timeInSecondsToNextBeat < fiftyWindow || timeInSecondsToLastBeat < fiftyWindow) return Accuracy.FIFTY;
        return Accuracy.MISS;
    }


    public enum Accuracy {
        THREE_HUNDRED, ONE_HUNDRED, FIFTY, MISS
    }
    
}
