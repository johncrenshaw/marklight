#region Using Statements
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
using System.Collections;
#endregion

namespace MarkLight.Views.UI
{
    /// <summary>
    /// HyperLink view. 
    /// </summary>
    /// <d>Displays text that can be pressed. Has the states: Default, Highlighted, Pressed and Disabled.</d>
    [HideInPresenter]
    public class HyperLink : Label
    {
        #region Fields

        /// <summary>
        /// Indicates if the hyperlink is disabled.
        /// </summary>
        /// <d>If set to true the hyperlink enters the Disabled state and can't be interacted with.</d>
        [ChangeHandler("IsDisabledChanged")]
        public _bool IsDisabled;

        /// <summary>
        /// Boolean indicating if the hyperlink is being pressed.
        /// </summary>
        [NotSetFromXuml]
        public bool IsPressed;

        /// <summary>
        /// Boolean indicating if mouse is over the hyperlink.
        /// </summary>
        [NotSetFromXuml]
        public bool IsMouseOver;
                
        #endregion

        #region Methods

        /// <summary>
        /// Sets default values of the view.
        /// </summary>
        public override void SetDefaultValues()
        {
            base.SetDefaultValues();

            Width.DirectValue = new ElementSize(120);
            Height.DirectValue = new ElementSize(40);
            FontColor.Value = ColorValueConverter.ColorCodes["lightblue"]; 
        }

        public void UpdateState()
        {
            if (IsDisabled)
            {
                SetState("Disabled");
            }
            else if (IsFocused)
            {
                if (IsPressed)
                {
                    SetState("FocusedPressed");
                }
                else
                {
                    SetState("Focused");
                }
            }
            else if (IsMouseOver)
            {
                if (IsPressed)
                {
                    SetState("Pressed");
                }
                else
                {
                    SetState("Highlighted");
                }
            }
            else
            {
                SetState(DefaultStateName);
            }
        }

        /// <summary>
        /// Called when IsDisabled field changes.
        /// </summary>
        public virtual void IsDisabledChanged()
        {
            UpdateState();

            if (IsDisabled)
            {
                // disable hyperlink actions
                Click.IsDisabled = true;
                MouseEnter.IsDisabled = true;
                MouseExit.IsDisabled = true;
                MouseDown.IsDisabled = true;
                MouseUp.IsDisabled = true;
            }
            else
            {
                // enable hyperlink actions
                Click.IsDisabled = false;
                MouseEnter.IsDisabled = false;
                MouseExit.IsDisabled = false;
                MouseDown.IsDisabled = false;
                MouseUp.IsDisabled = false;
            }
        }

        /// <summary>
        /// Called when mouse enters.
        /// </summary>
        public void HyperLinkMouseEnter()
        {
            if (State == "Disabled")
                return;

            IsMouseOver = true;
            UpdateState();
        }

        /// <summary>
        /// Called when mouse exits.
        /// </summary>
        public void HyperLinkMouseExit()
        {
            if (State == "Disabled")
                return;

            IsMouseOver = false;
            UpdateState();
        }

        /// <summary>
        /// Called when mouse down.
        /// </summary>
        public void HyperLinkMouseDown()
        {
            if (State == "Disabled")
                return;

            if (!IsFocused)
            {
                Focus();
            }

            IsPressed = true;
            UpdateState();
        }

        /// <summary>
        /// Called when mouse up.
        /// </summary>
        public void HyperLinkMouseUp()
        {
            if (State == "Disabled")
                return;

            IsPressed = false;
            UpdateState();
        }

        /// <summary>
        /// Non-Propagating input event handler. Called on a view when it focuses.
        /// </summary>
        public override void HandleFocus()
        {
            base.HandleFocus();

            UpdateState();
        }

        /// <summary>
        /// Non-Propagating input event handler. Called on a view when it blurs.
        /// </summary>
        public override void HandleBlur()
        {
            base.HandleBlur();

            UpdateState();
        }

        /// <summary>
        /// Propagating input event handler. Called on a view to trigger the action.
        /// </summary>
        public override bool HandleAction()
        {
            base.HandleAction();

            // Trigger the click
            Click.Trigger();

            // Visibly show as presed for a moment
            IsPressed = true;
            UpdateState();
            StartCoroutine(ReleaseButtonShortly());

            return false;
        }

        private IEnumerator ReleaseButtonShortly()
        {
            yield return new WaitForSeconds(0.3f);
            IsPressed = false;
            UpdateState();
        }

        #endregion
    }
}
