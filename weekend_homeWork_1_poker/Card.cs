using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weekend_homeWork_1_poker
{
  // 문양
  public enum Suit
  {
    Clubs,
    Hearts,
    Diamonds,
    Spades
  }
  // 숫자
  public enum Rank
  {
    Two = 2,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace
  }

  // 카드
  class Card
  {
    public Suit Suit { get; }
    public Rank Rank { get; }

    public Card(Suit suit, Rank rank)
    {
      Suit = suit;
      Rank = rank;
    }

    public static string GetCardDisplayString(Card card)
    {
      string suitSymbol;
      switch (card.Suit)
      {
        case Suit.Spades:
          suitSymbol = "♠";
          break;
        case Suit.Diamonds:
          suitSymbol = "◆";
          break;
        case Suit.Hearts:
          suitSymbol = "♥";
          break;
        case Suit.Clubs:
          suitSymbol = "♣";
          break;
        default:
          suitSymbol = "?";
          break;
      }

      string rankName;
      switch (card.Rank)
      {
        case Rank.Ace:
          rankName = "A";
          break;
        case Rank.King:
          rankName = "K";
          break;
        case Rank.Queen:
          rankName = "Q";
          break;
        case Rank.Jack:
          rankName = "J";
          break;
        default:
          rankName = ((int)card.Rank).ToString();
          break;
      }

      return $"{suitSymbol} {rankName}";
    }
  }
}
