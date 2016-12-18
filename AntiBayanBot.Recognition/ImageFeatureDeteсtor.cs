using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.Structure;

namespace AntiBayanBot.Recognition
{
    public class ImageFeatureDeteсtor
    {
        private readonly SURFDetector _surfDetector = new SURFDetector(300, false);


        public float[,] GetDescriptors(Bitmap bitmap)
        {
            using (var image = new Image<Gray, byte>(bitmap))
            {
                var modelKeyPoints = _surfDetector.DetectKeyPointsRaw(image, null);
                return _surfDetector.ComputeDescriptorsRaw(image, null, modelKeyPoints).Data;
            }
        }


        public float IsSimilar(float[,] firstImageDescriptorsData, float[,] secondimageDescriptorsData)
        {
            var similarity1 = CalculateSimilarity(firstImageDescriptorsData, secondimageDescriptorsData);
            var similarity2 = CalculateSimilarity(secondimageDescriptorsData, firstImageDescriptorsData);

            return Math.Max(similarity1, similarity2);
        }


        private float CalculateSimilarity(float[,] firstImageDescriptorsData, float[,] secondimageDescriptorsData)
        {
            var firstImageDescriptors = new Matrix<float>(firstImageDescriptorsData);
            var secondImageDescriptors = new Matrix<float>(secondimageDescriptorsData);

            // http://romovs.github.io/blog/2013/07/05/matching-image-to-a-set-of-images-with-emgu-cv/
            var indices = new Matrix<int>(secondImageDescriptors.Rows, 2); // matrix that will contain indices of the 2-nearest neighbors found
            var dists = new Matrix<float>(secondImageDescriptors.Rows, 2); // matrix that will contain distances to the 2-nearest neighbors found

            // create FLANN index with 4 kd-trees and perform KNN search over it look for 2 nearest neighbours
            var flannIndex = new Index(firstImageDescriptors, 4);
            flannIndex.KnnSearch(secondImageDescriptors, indices, dists, 2, 24);

            var similarity = 0;

            for (var i = 0; i < indices.Rows; i++)
            {
                // filter out all inadequate pairs based on distance between pairs
                if (dists.Data[i, 0] < 0.6 * dists.Data[i, 1])
                {
                    if (0 <= i && dists.Rows >= i)
                    {
                        similarity++;
                    }
                }
            }

            return (float)similarity / indices.Rows;
        }
    }
}