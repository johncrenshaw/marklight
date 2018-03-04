#region Using Statements
using MarkLight;
using MarkLight.ValueConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#endregion

namespace MarkLight.Views.UI
{
    /// <summary>
    /// View that holds user interface views.
    /// </summary>
    /// <d>Represents a root UICanvas containing a user interface in the scene.</d>
    [HideInPresenter]
    public class UserInterface : UICanvas
    {
        #region Fields

        private UIView _focusedView;
        private HashSet<UIView> _focusableViews = new HashSet<UIView>();
        private List<UIView> _sortedFocusableViews = null;
        private int _focusedViewIndex = -1;
        private bool _axisStarted = false;

        #endregion

        #region Methods

        private void Update()
        {
            if (Input.anyKeyDown)
            {
                Propagate(_focusedView, (v) => v.HandleKeyDown());
            }
            if (Input.anyKey)
            {
                Propagate(_focusedView, (v) => v.HandleKey());
            }

            // Reduce axis inputs to discrete events
            if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
            {
                if (!_axisStarted)
                {
                    Propagate(_focusedView, (v) => v.HandleAxisStart());
                    _axisStarted = true;
                }
                Propagate(_focusedView, (v) => v.HandleAxis());
            }
            else
            {
                _axisStarted = false;
            }
        }

        /// <summary>
        /// Initializes the view.
        /// </summary>
        public override void Initialize()
        {
            LayoutRoot = this;
            base.Initialize();
        }

        /// <summary>
        /// Move input focus to a specific view if it is not already focused
        /// </summary>
        public void Focus(UIView view)
        {
            if (view != null)
            {
                if (_focusedView != view)
                {
                    UIView oldFocusedView = _focusedView;
                    _focusedView = view;

                    // Clear cached values
                    _focusedViewIndex = -1;

                    if (oldFocusedView)
                    {
                        oldFocusedView.HandleBlur();
                    }
                    _focusedView.HandleFocus();
                }
            }
            else
            {
                Blur(_focusedView);
            }
        }

        /// <summary>
        /// Move input focus to a specific view by index in the list of focusable views, if it is not already focused
        /// </summary>
        public void Focus(int index)
        {
            if (index >= 0 && index < this.SortedFocusableViews.Count)
            {
                UIView view = this.SortedFocusableViews[index];
                if (_focusedView != view)
                {
                    UIView oldFocusedView = _focusedView;
                    _focusedView = view;
                    _focusedViewIndex = index;
                    if (oldFocusedView != null)
                    {
                        oldFocusedView.HandleBlur();
                    }
                    _focusedView.HandleFocus();
                }
            }
            else
            {
                Blur(_focusedView);
            }
        }

        /// <summary>
        /// Move input focus to the view before the currently focused view, or focus the last view if no view has focus
        /// </summary>
        public void FocusPrev()
        {
            if (this.SortedFocusableViews.Count == 0)
            {
                // Nothing to focus
                return;
            }

            if (FocusedViewIndex <= 0)
            {
                Focus(this.SortedFocusableViews.Count - 1);
            }
            else
            {
                Focus(FocusedViewIndex - 1);
            }
        }

        /// <summary>
        /// Move input focus to the view after the currently focused view, or focus the first view if no view has focus
        /// </summary>
        public void FocusNext()
        {
            if (this.SortedFocusableViews.Count == 0)
            {
                // Nothing to focus
                return;
            }

            if (FocusedViewIndex < 0 || FocusedViewIndex >= this.SortedFocusableViews.Count - 1)
            {
                Focus(0);
            }
            else
            {
                Focus(FocusedViewIndex + 1);
            }
        }

        /// <summary>
        /// Remove input focus from a specific view if it is focused
        /// </summary>
        public void Blur(UIView view)
        {
            if (_focusedView != null && _focusedView == view)
            {
                UIView oldFocusedView = _focusedView;
                _focusedView = null;
                _focusedViewIndex = -1;
                oldFocusedView.HandleBlur();
            }
        }

        /// <summary>
        /// Return true if the view has the input focus, false otherwise
        /// </summary>
        public bool IsFocused(UIView view)
        {
            return _focusedView == view;
        }

        /// <summary>
        /// Add a view to the tab order
        /// </summary>
        public void AddFocusableView(UIView view)
        {
            _focusableViews.Add(view);

            // Clear cached values
            _sortedFocusableViews = null;
            _focusedViewIndex = -1;
        }

        /// <summary>
        /// Remove a view from the tab order
        /// </summary>
        public void RemoveFocusableView(UIView view)
        {
            int index = SortedFocusableViews.IndexOf(view);
            _focusableViews.Remove(view);
            SortedFocusableViews.Remove(view);
            if (_focusedView == view)
            {
                if (index + 1 < this.SortedFocusableViews.Count)
                {
                    UIView newView = this.SortedFocusableViews[index + 1];
                    if (_focusedView != newView)
                    {
                        UIView oldFocusedView = _focusedView;
                        _focusedView = newView;
                        _focusedViewIndex = index + 1;
                        if (oldFocusedView != null)
                        {
                            oldFocusedView.HandleBlur();
                        }
                        _focusedView.HandleFocus();
                    }
                }
                else if (index > 0)
                {
                    UIView newView = this.SortedFocusableViews[index - 1];
                    if (_focusedView != newView)
                    {
                        UIView oldFocusedView = _focusedView;
                        _focusedView = newView;
                        _focusedViewIndex = index - 1;
                        if (oldFocusedView != null)
                        {
                            oldFocusedView.HandleBlur();
                        }
                        _focusedView.HandleFocus();
                    }
                }
                else
                {
                    UIView oldFocusedView = _focusedView;
                    _focusedView = null;
                    _focusedViewIndex = -1;
                    oldFocusedView.HandleBlur();
                }
            }

            // Clear cached values
            _sortedFocusableViews = null;
            _focusedViewIndex = -1;
        }

        /// <summary>
        /// Propagating input event handler. Return true to propagate or false to stop propagation.
        /// </summary>
        public override bool HandleKeyDown()
        {
            // Handle tab and shift tab
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    FocusPrev();
                }
                else
                {
                    FocusNext();
                }
            }

            // Handle form submission on Enter
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SubmitFocused();
            }

            // Handle click-like action on Space
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ActionFocused();
            }

            return false;
        }

        /// <summary>
        /// Propagating input event handler. Return true to propagate or false to stop propagation.
        /// </summary>
        public override bool HandleKey()
        {
            return false;
        }

        /// <summary>
        /// Propagating input event handler.
        /// </summary>
        public override bool HandleAxisStart()
        {
            // TODO: Use releative spatial position rather than tab order
            // to navigate by axis.
            if (Input.GetAxisRaw("Vertical") > 0)
            {
                FocusPrev();
                return false;
            }
            if (Input.GetAxisRaw("Vertical") < 0)
            {
                FocusNext();
                return false;
            }

            return false;
        }

        /// <summary>
        /// Propagating input event handler. Return true to propagate or false to stop propagation.
        /// </summary>
        public override bool HandleSubmit()
        {
            return false;
        }

        /// <summary>
        /// Propagating input event handler. Return true to propagate or false to stop propagation.
        /// </summary>
        public override bool HandleAction()
        {
            return false;
        }

        /// <summary>
        /// Calls delegate on the view and parents until the delegate returns false or there are no more parents.
        /// </summary>
        public delegate bool PropagationHandler(UIView view);
        private void Propagate(UIView view, PropagationHandler handler)
        {
            if (view != null)
            {
                UIView inputView = view;
                bool propagate = true;
                while (inputView && propagate)
                {
                    propagate = handler(inputView);
                    if (propagate)
                    {
                        inputView = inputView.LayoutParent as UIView;
                    }
                }
            }
            else
            {
                // if no view is focused, this component still gets any events that propagate
                handler(this);
            }
        }

        /// <summary>
        /// Propagating input event handler. Return true to propagate or false to stop propagation.
        /// </summary>
        public void SubmitFocused()
        {
            Propagate(_focusedView, (v) => v.HandleSubmit());
        }

        /// <summary>
        /// Propagating input event handler. Return true to propagate or false to stop propagation.
        /// </summary>
        public void ActionFocused()
        {
            Propagate(_focusedView, (v) => v.HandleAction());
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the list of focusable views as a sorted array.
        /// </summary>
        public List<UIView> SortedFocusableViews
        {
            get
            {
                if (_sortedFocusableViews == null)
                {
                    _sortedFocusableViews = new List<UIView>();
                    foreach (UIView view in _focusableViews)
                    {
                        _sortedFocusableViews.Add(view);
                    }
                    _sortedFocusableViews.Sort(new UIViewOrderComparer());
                    _focusedViewIndex = -1;
                }

                return _sortedFocusableViews;
            }
        }

        /// <summary>
        /// Returns the index of the currently focused view, or -1 if no view is focused.
        /// </summary>
        public int FocusedViewIndex
        {
            get
            {
                if (_focusedViewIndex == -1 && _focusedView != null)
                {
                    _focusedViewIndex = _sortedFocusableViews.IndexOf(_focusedView);
                }

                return _focusedViewIndex;
            }
        }

        #endregion
    }
}
