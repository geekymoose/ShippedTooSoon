using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Score is the x first best times. (Set to 5 best scores)
 * \author  Constantin Masson
 */
[CreateAssetMenu(fileName = "ScoreDataSave", menuName = "ScoreDataSave", order = 1)]
public class ScoreData : ScriptableObject {

    // -------------------------------------------------------------------------
    // Data (Unity editor)
    // -------------------------------------------------------------------------
    [SerializeField]
    [Tooltip("The scores. Don't enter them manually (Added at victory). Format: (min:sec:mil)")]
    private float[] scores = new float[5]; // 5 best times (min:sec:mil)


    // -------------------------------------------------------------------------
    // Internal data
    // -------------------------------------------------------------------------
    private float noScoreData = 999;
    

    // -------------------------------------------------------------------------
    // Data Methods
    // -------------------------------------------------------------------------

    /**
     * Add a score entry.
     * Accept duplicate.
     * Returns the position where new score has been added (pos 0 = best score)
     * or -1 if not added (Do better dude, you will make it ;).
     *
     * Don't give negative score! It will actually be added (But doesn't make
     * any sense)
     */
    public float addScoreEntry(float value) {
        int pos = scores.Length - 1;
        bool inPodium = (value < scores[scores.Length-1]) ? true : false;

        if(inPodium) {
            while(pos > 0 && value < scores[pos - 1]) {
                scores[pos] = scores[pos - 1];
                --pos;
            }
            scores[pos] = value;
            return pos;
        }
        return -1;
    }

    public void resetScores() {
        for(int k = 0; k < this.scores.Length; ++k) {
            this.scores[k] = noScoreData;
        }
    }

    /**
     * Return a string with all scores sorted by best time.
     * Best score is first displayed.
     * Return line between each score.
     */
    public string getScoreDataAsString() {
        string str = "";
        for(int k = 0; k < scores.Length; ++k) {
            str += (scores[k] == noScoreData) ? "empty\n" : formatScoreTimestamp(scores[k]) + "\n";
        }
        return str;
    }


    // -------------------------------------------------------------------------
    // Static functions
    // -------------------------------------------------------------------------

    /**
     * Formatted version of a timestamp.
     * Timestamp is the number of seconds.
     * Maximum value is 99min 99 sec, undefined format result for highter value.
     * (Score timestamp is the time used to finish the game)
     */
    public static string formatScoreTimestamp(float timestamp) {
		int min = (int)(timestamp / 60f);
		int sec = (int)(timestamp % 60f);
        int mil = (int)((timestamp % 1) * 100);

		string timeStr = min.ToString("00") + ":" + sec.ToString("00") + ":" + mil.ToString("00");
        return timeStr;
    }
}
