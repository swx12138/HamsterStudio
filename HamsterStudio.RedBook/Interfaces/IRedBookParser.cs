using HamsterStudio.BeautyLegs.RedBook;

namespace HamsterStudio.RedBook.Interfaces;

public interface IRedBookParser
{
    NoteDataModel? GetNoteData(string url);
}
