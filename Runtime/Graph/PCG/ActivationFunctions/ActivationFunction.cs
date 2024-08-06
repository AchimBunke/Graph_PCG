/*
 * MIT License
 *
 * Copyright (c) 2024 Achim Bunke
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Linq;
using UnityEngine;
using UnityUtilities.Attributes;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space
{
    /// <summary>
    /// Used to convert a distance like feature or spatial distance into a weight. Lower distance means hihger result.
    /// They are resitant to 0 distance values.
    /// </summary>
    public abstract class ActivationFunction
    {
        public const float SafeEpsilon = 1e-5f;
        public abstract float Apply(float distance);
        public virtual float[] Apply(float[] distances)
        {
            float[] result = new float[distances.Length];
            for (int i = 0; i < distances.Length; ++i)
            {
                result[i] = Apply(distances[i]);
            }
            return result;
        }
    }
    [Serializable]

    public struct ActivationFunctionConfiguration
    {
        public ActivationFunctionType ActivationFunctionType;

        [ShowIf(EvaluationMode: ShowIfAttribute.EvaluationModeType.OR, HideInInspector: true,
            nameof(ShowSigmaField))]
        public float Sigma;

        private bool ShowSigmaField() => ActivationFunctionType switch
        {
            ActivationFunctionType.Gaussian => true,
            _ => false,
        };

        public ActivationFunction Create()
        {
            switch (ActivationFunctionType)
            {
                case ActivationFunctionType.Gaussian:
                    return new GaussianActivationFunction(Sigma);
                case ActivationFunctionType.ReLU:
                    return new ReLUActivationFunction();
                case ActivationFunctionType.Softmax:
                    return new SoftmaxActivationFunction();
                default:
                    throw new InvalidOperationException("ActivationFunctionType does not provide implementation");
            }
        }
    }
    public enum ActivationFunctionType
    {
        Gaussian,
        ReLU,
        Softmax
    }

    public class GaussianActivationFunction : ActivationFunction
    {
        private float _sigma;
        public GaussianActivationFunction(float sigma)
        {
            _sigma = sigma;
        }
        public override float Apply(float distance)
        {
            return Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * Mathf.Pow(_sigma, 2)));
        }
    }
    public class ReLUActivationFunction : ActivationFunction
    {


        public override float Apply(float distance)
        {
            return 1f / (distance + ActivationFunction.SafeEpsilon);
        }
    }
    public class SoftmaxActivationFunction : ActivationFunction
    {
        public override float Apply(float distance)
        {
            throw new InvalidOperationException("Softmax does not make sense to be used for single values");
        }
        public override float[] Apply(float[] distances)
        {
            float[] result = new float[distances.Length];
            float maxValue = -distances.Min();
            float sum = 0.0f;
            for (int i = 0; i < distances.Length; i++)
            {
                result[i] = Mathf.Exp(-distances[i] - maxValue);
                sum += result[i];
            }

            // Normalize
            for (int i = 0; i < distances.Length; i++)
            {
                result[i] /= sum;
            }

            return result;
        }

    }
}