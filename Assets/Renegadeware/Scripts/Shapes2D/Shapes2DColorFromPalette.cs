using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Renegadeware {
    public class Shapes2DColorFromPalette : M8.ColorFromPaletteBase {

        [HideInInspector]
        [SerializeField]
        int _indexOutline;
        [HideInInspector]
        [SerializeField]
        float _brightnessOutline = 1f;
        [HideInInspector]
        [SerializeField]
        float _alphaOutline = 1f;

        [HideInInspector]
        [SerializeField]
        int _index2;
        [HideInInspector]
        [SerializeField]
        float _brightness2 = 1f;
        [HideInInspector]
        [SerializeField]
        float _alpha2 = 1f;

        public Shapes2D.Shape target;

        public int indexOutline {
            get { return _indexOutline; }
            set {
                if(_indexOutline != value) {
                    _indexOutline = value;
                    ApplyColor();
                }
            }
        }

        public float brightnessOutline {
            get { return _brightnessOutline; }
            set {
                if(_brightnessOutline != value) {
                    _brightnessOutline = value;
                    ApplyColor();
                }
            }
        }

        public float alphaOutline {
            get { return _alphaOutline; }
            set {
                if(_alphaOutline != value) {
                    _alphaOutline = value;
                    ApplyColor();
                }
            }
        }

        public Color colorOutline {
            get {
                if(palette) {
                    var clr = palette.GetColor(_indexOutline);
                    return new Color(clr.r * _brightnessOutline, clr.g * _brightnessOutline, clr.b * _brightnessOutline, clr.a * _alphaOutline);
                }
                else
                    return Color.white;
            }
        }

        public int index2 {
            get { return _index2; }
            set {
                if(_index2 != value) {
                    _index2 = value;
                    ApplyColor();
                }
            }
        }

        public float brightness2 {
            get { return _brightness2; }
            set {
                if(_brightness2 != value) {
                    _brightness2 = value;
                    ApplyColor();
                }
            }
        }

        public float alpha2 {
            get { return _alpha2; }
            set {
                if(_alpha2 != value) {
                    _alpha2 = value;
                    ApplyColor();
                }
            }
        }

        public Color color2 {
            get {
                if(palette) {
                    var clr = palette.GetColor(_index2);
                    return new Color(clr.r * _brightness2, clr.g * _brightness2, clr.b * _brightness2, clr.a * _alpha2);
                }
                else
                    return Color.white;
            }
        }

        public override void ApplyColor() {
            if(!target)
                return;

            var setting = target.settings;
            setting.fillColor = color;
            setting.fillColor2 = color2;
            setting.outlineColor = colorOutline;
        }

        void Awake() {
            if(!target)
                target = GetComponent<Shapes2D.Shape>();
        }
    }
}