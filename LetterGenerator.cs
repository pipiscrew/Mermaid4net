namespace Mermaid4net
{
    internal class LetterGenerator : INodeHandleGenerator
    {
        private int currentIndex = 0;

        public string GetNextNodeHandle()
        {
            string nextLetter = ConvertToLetter(currentIndex);
            currentIndex++;
            return nextLetter;
        }

        private string ConvertToLetter(int index)
        {
            int letterCount = 1;
            int tempIndex = index;

            while (tempIndex >= 26)
            {
                tempIndex /= 26;
                letterCount++;
            }

            char[] letters = new char[letterCount];
            for (int i = letterCount - 1; i >= 0; i--)
            {
                letters[i] = (char)('A' + (index % 26));
                index /= 26;
            }

            return new string(letters);
        }
    }

    internal class NumGenerator : INodeHandleGenerator
    {
        private int currentIndex = 0;

        public string GetNextNodeHandle()
        {
            return currentIndex++.ToString();
        }
    }

    internal interface INodeHandleGenerator
    {
        string GetNextNodeHandle();
    }
}
