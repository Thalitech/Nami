using System.Text;
using Nami.Common;
using Nami.Modules.Games.Common;

namespace Nami.Modules.Games.Extensions
{
    public static class MinesweeperFieldExtensions
    {
        public static string ToEmojiString(this MinesweeperField field)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < field.Rows; i++) {
                for (int j = 0; j < field.Cols; j++) {
                    int count = field.Field[i, j];
                    sb.Append("||").Append(count == -1 ? Emojis.Bomb : Emojis.Numbers.Get(count)).Append("|| ");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
