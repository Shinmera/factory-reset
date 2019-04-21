using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace team5
{
    public class Transforms
    {
        private class MatrixStack{
            private List<Matrix> Stack;
            
            public MatrixStack()
            {
                Stack = new List<Matrix>();
                Stack.Add(Matrix.Identity);
            }
            
            public Matrix Top => Stack[Stack.Count-1];
            
            public Matrix Set(Matrix matrix)
            {
                Stack[Stack.Count-1] = matrix;
                return matrix;
            }
            
            public Matrix Reset()
            {
                return Set(Matrix.Identity);
            }
            
            public Matrix Push()
            {
                Stack.Add(Top);
                return Top;
            }
            
            public Matrix Pop()
            {
                if(1 < Stack.Count)
                {
                    Matrix top = Top;
                    Stack.RemoveAt(Stack.Count - 1);
                    return top;
                }
                else
                    return Reset();
            }
            
            public Matrix Translate(Vector2 by)
            {
                return Set(Top*Matrix.CreateTranslation(by.X, by.Y, 0));
            }

            public Matrix Translate(Vector3 by)
            {
                return Set(Top * Matrix.CreateTranslation(by.X, by.Y, 0));
            }

            public Matrix Scale(float by)
            {
                return Set(Top*Matrix.CreateScale(by, by, 1));
            }
            
            public Matrix Scale(Vector2 by)
            {
                return Set(Top*Matrix.CreateScale(by.X, by.Y, 1));
            }
            
            public Matrix Rotate(float radians)
            {
                return Set(Top*Matrix.CreateRotationZ(radians));
            }
            
            public Vector2 Transform(Vector2 vec)
            {
                return Vector2.Transform(vec, Top);
            }
        }
        
        private MatrixStack ViewStack = new MatrixStack();
        private MatrixStack ModelStack = new MatrixStack();
        
        public Matrix ProjectionMatrix = Matrix.Identity;
        
        /// <summary>
        ///   Returns the current View matrix.
        /// </summary>
        public Matrix ViewMatrix => ViewStack.Top;
        
        /// <summary>
        ///   Returns the current Model matrix.
        /// </summary>
        public Matrix ModelMatrix => ModelStack.Top;
        
        /// <summary>
        ///   Resets the Model matrix to an identity matrix.
        /// </summary>
        public Matrix Reset(){ return ModelStack.Reset(); }
        
        /// <summary>
        ///   Push the Model matrix stack, copying the previous matrix and preventing
        ///   it from being modified.
        /// </summary>
        public Matrix Push(){ return ModelStack.Push(); }
        
        /// <summary>
        ///   Pops the Model matrix stack, undoing the changes made since the last push.
        ///   If the stack would be made empty by this operation, a new identity matrix
        ///   is pushed on top.
        /// </summary>
        public Matrix Pop(){ return ModelStack.Pop(); }
        
        /// <summary>
        ///   Translate tho Model matrix by the given vector.
        /// </summary>
        public Matrix Translate(Vector2 by){ return ModelStack.Translate(by); }

        /// <summary>
        ///   Translate tho Model matrix by the given vector.
        /// </summary>
        public Matrix Translate(Vector3 by) { return ModelStack.Translate(by); }

        /// <summary>
        ///   Scale the Model matrix by the given float in both X and Y.
        /// </summary>
        public Matrix Scale(float by){ return ModelStack.Scale(by); }

        /// <summary>
        ///   Scale the Model matrix by the given float in X and Y.
        /// </summary>
        public Matrix Scale(float x, float y) { return ModelStack.Scale(new Vector2(x, y)); }
        
        /// <summary>
        ///   Scale the Model matrix by the given vector.
        /// </summary>
        public Matrix Scale(Vector2 by){ return ModelStack.Scale(by); }
        
        /// <summary>
        ///   Rotate the Model matrix by the given amount.
        /// </summary>
        public Matrix Rotate(float radians){ return ModelStack.Rotate(radians); }
        
        /// <summary>
        ///   Push the View matrix stack, copying the previous matrix and preventing
        ///   it from being modified.
        /// </summary>
        public Matrix PushView(){ return ViewStack.Push(); }
        
        /// <summary>
        ///   Pops the View matrix stack, undoing the changes made since the last push.
        ///   If the stack would be made empty by this operation, a new identity matrix
        ///   is pushed on top.
        /// </summary>
        public Matrix PopView(){ return ViewStack.Pop(); }
        
        /// <summary>
        ///   Reset the View matrix to an identity matryx.
        /// </summary>
        public Matrix ResetView(){ return ViewStack.Reset(); }
        
        /// <summary>
        ///   Translate the View matrix by the given vector.
        /// </summary>
        public Matrix TranslateView(Vector2 by){ return ViewStack.Translate(by); }
        
        /// <summary>
        ///   Scale the View matrix by the given float in both X and Y.
        /// </summary>
        public Matrix ScaleView(float by){ return ViewStack.Scale(by); }
        
        /// <summary>
        ///   Scale the View matrix by the given vector.
        /// </summary>
        public Matrix ScaleView(Vector2 by){ return ViewStack.Scale(by); }
        
        /// <summary>
        ///   Rotate the View matrix by the given amount.
        /// </summary>
        public Matrix RotateView(float radians){ return ViewStack.Rotate(radians); }
        
        /// <summary>
        ///   Transforms the given vector by the transform matrices.
        /// </summary>
        public Vector2 Transform(Vector2 vec)
        {
            return Vector2.Transform(ViewStack.Transform(ModelStack.Transform(vec)), ProjectionMatrix);
        }
        
        /// <summary>
        ///   Transforms the given vector by the transform matrices.
        ///   Returns the equivalent of: vec2(ViewMatrix*ModelMatrix*vec4(vec))
        /// </summary>
        public static Vector2 operator *(Transforms t, Vector2 vec)
        {
            return t.Transform(vec);
        }
    }
}
