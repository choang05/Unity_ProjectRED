﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Camera Transitions.
//
// Copyright (c) Ibuprogames <hello@ibuprogames.com>. All rights reserved.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// Only for promotional material.
//#define ENABLE_VIDEO_MODE

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CameraTransitions
{
  /// <summary>
  /// Demo source code. For a more easy example, load 'Demo/DemoAssistant.scene'.
  /// </summary>
  public sealed class DemoTransitions : MonoBehaviour
  {
    public bool guiShow = true;

    public bool showTransitionName = false;

    public float transitionTime = 1.0f;

    public float changeCameraTime = 0.0f;

    public bool randomTransition = true;

    private int transitionSelected = -1;

    private CameraTransition cameraTransition;

    private List<Camera> cameras = new List<Camera>();

    private List<CameraTransitionEffects> transitionEnum = new List<CameraTransitionEffects>();

    private List<bool> transitionToggles = new List<bool>();

    private int cameraIdx;

    private System.Random random = new System.Random(Guid.NewGuid().GetHashCode());

    // FPS counter.
    private float updateInterval = 0.5f;
    private float accum = 0.0f;
    private int frames = 0;
    private float timeleft;
    private float fps = 0.0f;

    private float changeTime = 0.0f;

    private bool menuOpen = false;

    private Vector2 scrollPosition = Vector2.zero;

    private GUIStyle effectNameStyle;
    private GUIStyle menuStyle;

    private const float guiMargen = 10.0f;
    private const float guiWidth = 200.0f;

    private void OnEnable()
    {
      cameras.AddRange(GameObject.FindObjectsOfType<Camera>());
      if (cameras.Count > 1)
      {
        cameraTransition = GameObject.FindObjectOfType<CameraTransition>();
        if (cameraTransition != null)
        {
          cameraTransition.Time = transitionTime;

          // Randomly select a camera.
          cameras.Sort(delegate(Camera a, Camera b) { return string.Compare(a.gameObject.name, b.gameObject.name); });

          cameraIdx = random.Next(0, cameras.Count - 1);

          for (int i = 0; i < (cameras.Count - 0); ++i)
            cameras[i].gameObject.SetActive(i == cameraIdx);

          // Test all effects.
          Array untestedTransitions = Enum.GetValues(typeof(CameraTransitionEffects));
          for (int i = 0; i < untestedTransitions.Length; ++i)
          {
            CameraTransitionEffects testedEffect = (CameraTransitionEffects)untestedTransitions.GetValue(i);

            if (cameraTransition.CheckTransition(testedEffect) == true)
              transitionEnum.Add(testedEffect);
            else
              Debug.LogWarningFormat("Transition '{0}' is not supported.", testedEffect.ToString());
          }

          for (int i = 0; i < transitionEnum.Count; ++i)
            transitionToggles.Add(false);
        }
        else
          Debug.LogWarning(@"CameraTransition not found.");
      }
      else
        Debug.LogWarning(@"Few cameras found (at least two).");

      this.enabled = (cameras.Count > 1 && cameraTransition != null);

#if ENABLE_VIDEO_MODE
      guiShow = false;
      changeCameraTime = 0.5f;
      randomTransition = false;
      cameraTransition.Time = 2.0f;
#endif
    }

    private void Update()
    {
      // Update FPS counter.
      timeleft -= Time.deltaTime;
      accum += Time.timeScale / Time.deltaTime;
      frames++;

      if (timeleft <= 0.0f)
      {
        fps = accum / frames;
        timeleft = updateInterval;
        accum = 0.0f;
        frames = 0;
      }

      // Auto switch cameras
      if (changeCameraTime > 0.0f && cameras.Count > 0)
      {
        changeTime += (cameraTransition.IsRunning ? 0.0f : Time.deltaTime);
        if (changeTime >= changeCameraTime)
        {
          int nextCamera = 0;
          if (cameras.Count > 2)
          {
            do
            {
              nextCamera = random.Next(0, cameras.Count - 1);
            } while (nextCamera == cameraIdx);
          }
          else
            nextCamera = (cameraIdx < cameras.Count - 1) ? cameraIdx + 1 : 0;

          SwitchCamera(nextCamera);

          changeTime = 0.0f;
        }
      }

      // Update input.
      if (cameraTransition.Progress == 0.0f)
      {
        if (Input.GetKeyUp(KeyCode.Tab) == true)
          guiShow = !guiShow;

        if (Input.GetKeyUp(KeyCode.LeftArrow) == true)
        {
          changeCameraTime = 0.0f;

          if (cameraIdx > 0)
            SwitchCamera(cameraIdx - 1);
          else
            SwitchCamera(cameras.Count - 1);
        }

        if (Input.GetKeyUp(KeyCode.RightArrow) == true)
        {
          changeCameraTime = 0.0f;

          if (cameraIdx < cameras.Count - 1)
            SwitchCamera(cameraIdx + 1);
          else
            SwitchCamera(0);
        }

        if (Input.GetKeyUp(KeyCode.Space) == true)
        {
          changeCameraTime = 0.0f;

          int nextCamera = 0;
          if (cameras.Count > 2)
          {
            do
            {
              nextCamera = random.Next(0, cameras.Count - 1);
            } while (nextCamera == cameraIdx);
          }
          else
            nextCamera = (cameraIdx < cameras.Count - 1) ? cameraIdx + 1 : 0;

          SwitchCamera(nextCamera);
        }
      }

#if !UNITY_WEBPLAYER
      if (Input.GetKeyDown(KeyCode.Escape))
        Application.Quit();
#endif
    }

    private void OnGUI()
    {
      //const float screenWidth = 960;
      //const float screenHeight = 600;
      //GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / screenWidth, Screen.height / screenHeight, 1.0f));

      if (effectNameStyle == null)
      {
        effectNameStyle = new GUIStyle(GUI.skin.textArea);
        effectNameStyle.alignment = TextAnchor.MiddleCenter;
        effectNameStyle.fontSize = 22;
      }

      if (menuStyle == null)
      {
        menuStyle = new GUIStyle(GUI.skin.textArea);
        menuStyle.alignment = TextAnchor.MiddleCenter;
        menuStyle.fontSize = 14;
      }

#if ENABLE_VIDEO_MODE
/*
      if (cameraTransition.IsRunning == true)
      {
        GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 150.0f, 20.0f, 300.0f, 30.0f), AddSpacesToName(cameraTransition.Transition.ToString()), effectNameStyle);
        GUILayout.EndArea();
      }
*/
#endif

      if (showTransitionName == true && guiShow == false && cameraTransition.IsRunning == true)
      {
        GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 150.0f, 20.0f, 300.0f, 30.0f), AddSpacesToName(cameraTransition.Transition.ToString()).ToUpper(), effectNameStyle);
        GUILayout.EndArea();
      }

      if (guiShow == false)
        return;

      GUILayout.BeginHorizontal(@"box", GUILayout.Width(Screen.width));
      {
        GUILayout.Space(guiMargen);

        if (GUILayout.Button(@"Menu", menuStyle, GUILayout.Width(60.0f)) == true)
          menuOpen = !menuOpen;

        GUILayout.FlexibleSpace();

        GUI.enabled = !cameraTransition.IsRunning;

        if (GUI.enabled == true && GUILayout.Button(@"<<<", menuStyle) == true)
        {
          changeCameraTime = 0.0f;

          if (cameraIdx > 0)
            SwitchCamera(cameraIdx - 1);
          else
            SwitchCamera(cameras.Count - 1);

          Event.current.Use();
        }

        GUI.contentColor = Color.white;

        if (GUILayout.Button(AddSpacesToName(GUI.enabled == true ? cameras[cameraIdx].gameObject.name : cameraTransition.Transition.ToString()).ToUpper(), menuStyle, GUILayout.Width(200.0f)) == true)
        {
          changeCameraTime = 0.0f;

          int randomCamera = 0;
          do
          {
            randomCamera = random.Next(0, cameras.Count - 1);
          } while (randomCamera == cameraIdx);

          SwitchCamera(randomCamera);
        }

        if (GUI.enabled == true && GUILayout.Button(@">>>", menuStyle) == true)
        {
          changeCameraTime = 0.0f;

          if (cameraIdx < cameras.Count - 1)
            SwitchCamera(cameraIdx + 1);
          else
            SwitchCamera(0);
        }

        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        if (fps < 30.0f)
          GUI.contentColor = Color.yellow;
        else if (fps < 15.0f)
          GUI.contentColor = Color.red;
        else
          GUI.contentColor = Color.green;

        GUILayout.Label(fps.ToString("000"), menuStyle, GUILayout.Width(40.0f));

        GUI.contentColor = Color.white;

        GUILayout.Space(guiMargen);
      }
      GUILayout.EndHorizontal();

      if (menuOpen == true)
      {
        GUILayout.BeginVertical("box", GUILayout.Width(guiWidth));
        {
          GUILayout.Space(guiMargen);

          GUILayout.BeginHorizontal();
          {
            GUILayout.Label("Time", GUILayout.Width(40));

            cameraTransition.Time = GUILayout.HorizontalSlider(cameraTransition.Time, 0.1f, 5.0f);
          }
          GUILayout.EndHorizontal();

          GUILayout.Space(10.0f);

          bool randomChanged = GUILayout.Toggle(randomTransition, @" Random transition");
          if (randomChanged != randomTransition)
          {
            randomTransition = randomChanged;

            if (randomTransition == true)
              transitionSelected = -1;
          }

          scrollPosition = GUILayout.BeginScrollView(scrollPosition, "box");
          {
            for (int i = 0; i < transitionEnum.Count; ++i)
            {
              bool enableChanged = GUILayout.Toggle(transitionToggles[i], @" " + transitionEnum[i].ToString());
              if (enableChanged != transitionToggles[i])
              {
                transitionSelected = i;

                randomTransition = false;
              }
            }
          }
          GUILayout.EndScrollView();

          for (int i = 0; i < transitionEnum.Count; ++i)
            transitionToggles[i] = (i == transitionSelected);

          GUILayout.Space(guiMargen);
        }
        GUILayout.EndVertical();
      }
    }

    /// <summary>
    /// Switch camera with transition effect.
    /// </summary>
    private void SwitchCamera(int idx)
    {
      CameraTransitionEffects transition = cameraTransition.Transition;

      // Add some random parameters (optional).
      List<object> parameters = new List<object>();

#if ENABLE_VIDEO_MODE
      if ((int)transition < transitionEnum.Count - 1)
        transition++;
      else
        transition = 0;

      transition = CameraTransitionEffects.WarpWave;
#else
      if (randomTransition == true)
      {
          do
          {
            transition = transitionEnum[random.Next(transitionEnum.Count)];
          } while (transition == cameraTransition.Transition);
      }
      else
      {
        if (transitionSelected == -1)
          transitionSelected = 0;

        transition = transitionEnum[transitionSelected];
      }

      switch (transition)
      {
        case CameraTransitionEffects.Cube:
          parameters.Add(UnityEngine.Random.Range(0.5f, 0.9f));                         // Perspective.
          parameters.Add(UnityEngine.Random.Range(0.0f, 0.8f));                         // Zoom.
          parameters.Add(UnityEngine.Random.Range(0.2f, 0.6f));                         // Reflection.
          parameters.Add(UnityEngine.Random.Range(0.0f, 4.0f));                         // Elevantion.
          break;

        case CameraTransitionEffects.Doom:
          parameters.Add(UnityEngine.Random.Range(1, 20));                              // Bar width.
          parameters.Add(UnityEngine.Random.Range(1.0f, 5.0f));                         // Amplitude.
          parameters.Add(UnityEngine.Random.Range(0.0f, 0.25f));                        // Noise.
          parameters.Add(UnityEngine.Random.Range(0.5f, 3.0f));                         // Frequency.
          break;

        case CameraTransitionEffects.FadeToColor:
          parameters.Add(UnityEngine.Random.Range(0.2f, 0.4f));                         // Strength.
          parameters.Add(UnityEngine.Random.value > 0.5f ? Color.black : Color.white);  // Color.
          break;

        case CameraTransitionEffects.FadeToGrayscale:
          parameters.Add(UnityEngine.Random.Range(0.2f, 0.6f));                         // Strength.
          break;

        case CameraTransitionEffects.Flash:
          parameters.Add(UnityEngine.Random.Range(0.2f, 0.4f));                         // Strength.
          parameters.Add(UnityEngine.Random.Range(2.0f, 4.0f));                         // Intensity.
          parameters.Add(UnityEngine.Random.Range(0.4f, 0.6f));                         // Zoom.
          parameters.Add(UnityEngine.Random.Range(2.0f, 4.0f));                         // Velocity.
          parameters.Add(new Color(UnityEngine.Random.Range(0.8f, 1.0f),
                                   UnityEngine.Random.Range(0.7f, 0.9f),
                                   UnityEngine.Random.Range(0.2f, 0.4f)));              // Color.
          break;

        case CameraTransitionEffects.Flip:
          parameters.Add(UnityEngine.Random.value > 0.5f ? CameraTransitionFlip.Modes.Horizontal : CameraTransitionFlip.Modes.Vertical); // Mode.
          break;

        case CameraTransitionEffects.Fold:
          parameters.Add(UnityEngine.Random.value > 0.5f ? CameraTransitionFold.Modes.Horizontal : CameraTransitionFold.Modes.Vertical); // Mode.
          break;

        case CameraTransitionEffects.Glitch:
          parameters.Add(UnityEngine.Random.Range(0.3f, 0.6f));                         // Strength.
          break;

        case CameraTransitionEffects.LinearBlur:
          parameters.Add(UnityEngine.Random.Range(0.1f, 0.25f));                        // Intensity.
          parameters.Add(UnityEngine.Random.Range(6, 8));                               // Passes.
          break;

        case CameraTransitionEffects.Mosaic:
          Vector2 jumpTo = Vector2.zero;
          while (jumpTo == Vector2.zero)
            jumpTo = new Vector2(Mathf.Floor(UnityEngine.Random.Range(-2.0f, 2.0f)),
                                 Mathf.Floor(UnityEngine.Random.Range(-2.0f, 2.0f)));

          parameters.Add(jumpTo);                                                       // Steps.
          parameters.Add(UnityEngine.Random.value > 0.5f);                              // Rotation.
          break;

        case CameraTransitionEffects.PageCurl:
          parameters.Add(UnityEngine.Random.Range(0.0f, 90.0f));                        // Angle.
          parameters.Add(UnityEngine.Random.Range(0.1f, 0.5f));                         // Radius.
          parameters.Add(true);                                                         // Shadows.
          break;

        case CameraTransitionEffects.Pixelate:
          parameters.Add(UnityEngine.Random.Range(5.0f, 100.0f));                       // Pixelate size.
          break;
        
        case CameraTransitionEffects.Radial:
          parameters.Add(UnityEngine.Random.value > 0.5f);                              // Clockwise?
          break;

        case CameraTransitionEffects.RandomGrid:
          parameters.Add(UnityEngine.Random.Range(5, 10));                              // Rows.
          parameters.Add(UnityEngine.Random.Range(5, 10));                              // Columns.
          parameters.Add(UnityEngine.Random.Range(0.25f, 0.75f));                       // Smoothness.
          break;

        case CameraTransitionEffects.SmoothCircle:
          parameters.Add(UnityEngine.Random.value > 0.5f);                              // Invert?
          parameters.Add(UnityEngine.Random.Range(0.1f, 0.4f));                         // Smoothness.
          break;

        case CameraTransitionEffects.SmoothLine:
          parameters.Add(UnityEngine.Random.Range(0.0f, 359.9f));                       // Angle.
          parameters.Add(UnityEngine.Random.Range(0.1f, 0.4f));                         // Smoothness.
          break;
        
        case CameraTransitionEffects.Valentine:
          parameters.Add(UnityEngine.Random.Range(10.0f, 50.0f));                       // Border.
          parameters.Add(Color.red * UnityEngine.Random.Range(0.75f, 1.0f));            // Color.
          break;

        case CameraTransitionEffects.WarpWave:
          parameters.Add(UnityEngine.Random.value > 0.5f ? CameraTransitionWarpWave.Modes.Horizontal : CameraTransitionWarpWave.Modes.Vertical); // Mode.
          parameters.Add(UnityEngine.Random.Range(0.1f, 2.0f));                         // Curvature.
          break;
      }
#endif

      cameraTransition.DoTransition(transition, cameras[cameraIdx], cameras[idx], cameraTransition.Time, parameters.ToArray());

      cameraIdx = idx;
    }

    private string AddSpacesToName(string name)
    {
      return System.Text.RegularExpressions.Regex.Replace(name, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
    }
  }
}