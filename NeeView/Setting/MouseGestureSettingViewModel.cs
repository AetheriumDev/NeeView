﻿using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeeView.Setting
{
    /// <summary>
    /// MouseGestureSetting ViewModel
    /// </summary>
    public class MouseGestureSettingViewModel : BindableBase
    {
        private readonly IDictionary<string, CommandElement> _commandMap;
        private readonly string _key;
        private readonly TouchInputForGestureEditor _touchGesture;
        private readonly MouseInputForGestureEditor _mouseGesture;

        /// <summary>
        /// Property: GestureToken
        /// </summary>
        private GestureToken _gestureToken = new();
        public GestureToken GestureToken
        {
            get { return _gestureToken; }
            set { if (_gestureToken != value) { _gestureToken = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Property: Original Gesture
        /// </summary>
        public string OriginalGesture { get; set; }

        /// <summary>
        /// NewGesture property.
        /// </summary>
        private string _NewGesture = "";
        public string NewGesture
        {
            get { return _NewGesture; }
            set { if (_NewGesture != value) { _NewGesture = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="gestureSender"></param>
        public MouseGestureSettingViewModel(IDictionary<string, CommandElement> commandMap, string key, FrameworkElement gestureSender)
        {
            _commandMap = commandMap;
            _key = key;

            _touchGesture = new TouchInputForGestureEditor(gestureSender);
            _touchGesture.Gesture.GestureProgressed += Gesture_MouseGestureProgressed;

            _mouseGesture = new MouseInputForGestureEditor(gestureSender);
            _mouseGesture.Gesture.GestureProgressed += Gesture_MouseGestureProgressed;

            OriginalGesture = NewGesture = _commandMap[_key].MouseGesture;
            UpdateGestureToken(NewGesture);
        }

        /// <summary>
        /// Gesture Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gesture_MouseGestureProgressed(object? sender, MouseGestureEventArgs e)
        {
            NewGesture = e.Sequence.ToString();
            UpdateGestureToken(NewGesture);
        }


        /// <summary>
        /// Update Gesture Information
        /// </summary>
        /// <param name="gesture"></param>
        public void UpdateGestureToken(string gesture)
        {
            // Check Conflict
            var token = new GestureToken(gesture);

            if (!string.IsNullOrEmpty(token.Gesture))
            {
                token.Conflicts = _commandMap
                    .Where(i => i.Key != _key && i.Value.MouseGesture == token.Gesture)
                    .Select(i => i.Key)
                    .ToList();

                if (token.Conflicts.Count > 0)
                {
                    token.OverlapsText = string.Format(Properties.Resources.Notice_Conflict, ResourceService.Join(token.Conflicts.Select(i => CommandTable.Current.GetElement(i).Text)));
                }
            }

            GestureToken = token;
        }


        /// <summary>
        /// 決定
        /// </summary>
        public void Flush()
        {
            _commandMap[_key].MouseGesture = NewGesture;
        }


        /// <summary>
        /// Command: ClearCommand
        /// </summary>
        private RelayCommand? _clearCommand;
        public RelayCommand ClearCommand
        {
            get { return _clearCommand = _clearCommand ?? new RelayCommand(ClearCommand_Executed); }
        }

        private void ClearCommand_Executed()
        {
            _commandMap[_key].MouseGesture = "";
            _mouseGesture.Gesture.Reset();
        }
    }
}
