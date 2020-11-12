﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pixar
{
    [System.Serializable]
    class ShadowSettings
    {
        public enum MapSize
        {
            _256 = 256, _512 = 512, _1024 = 1024,
            _2048 = 2048, _4096 = 4096, _8192 = 8192
        }
        public enum FilterMode
        {
            PCF2x2, PCF3x3, PCF5x5, PCF7x7
        }
        [Min(0.001f)]
        public float maxDistance = 100f;

        [Range(0.001f, 1f)]
        public float distanceFade = 0.1f;

        [System.Serializable]
        public struct Directional
        {

            public MapSize atlasSize;

            public FilterMode filter;

            [Range(1, 4)]
            public int cascadeCount;

            [Range(0f, 1f)]
            public float cascadeRatio1, cascadeRatio2, cascadeRatio3;

            public Vector3 CascadeRatios =>
                new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);

            [Range(0.001f, 1f)]
            public float cascadeFade;

            public enum CascadeBlendMode
            {
                Hard, Soft, Dither
            }

            public CascadeBlendMode cascadeBlend;
        }
        public Directional directional = new Directional
        {
            atlasSize = MapSize._1024,
            filter = FilterMode.PCF2x2,
            cascadeCount = 4,
            cascadeRatio1 = 0.1f,
            cascadeRatio2 = 0.25f,
            cascadeRatio3 = 0.5f,
            cascadeFade = 0.1f,
            cascadeBlend = Directional.CascadeBlendMode.Hard
        };
    }
}
