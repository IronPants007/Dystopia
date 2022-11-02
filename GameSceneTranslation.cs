using System.Collections.Generic;
/*
#scene: main 
#background: outside1 
#characters: bob, jane
jane: "   !"
bob: "Hi there! How's your day going?"
+ "Good :)"
    bob: "Yayy!"
    -> END
+ { .happiness < 0 } Bad!!!
    bob: "Oh noooo :("
    { .supplies += 5 }
    bob: "Here's some supplies :)"
    -> END
*/

/*
Setting("scene") Colon Identifier("main") EndStatement
Setting("background") Colon Identifier("outside1") EndStatement
Setting("characters") Colon Identifier("bob") Comma Identifier("jane") EndStatement
Identifier("jane") Colon String("   !") EndStatement
Identifier("bob") Colon String("Hi there! How's your day going?") EndStatement
ChoiceDef String("Good :)") EndStatement
IndentChange(1)
Identifier("bob") Colon String("Yayy!") EndStatement
Arrow Identifier("END") EndStatement
IndentChange(0)
ChoiceDef Code(".happiness < 0") String("Bad!!!") EndStatement
IndentChange(1)
Identifier("bob") Colon String("Oh noooo :(") EndStatement
Code(".supplies += 5") EndStatement
Identifier("bob") Colon String("Here's some supplies :)") EndStatement
Arrow Identifier("END") EndStatement
*/


public class GameSceneTranslater {
    // Script -> Token List
    public static List<Token> Tokenize(string input) {
        List<Token> tokenList = new List<Token>();
        int lineIndex = 0;
        int lineNumber = 1;
        for (int i = 0; i < input.Length; i++) {
            char c = input[i];
            UnityEngine.Debug.Log(c);
            if (IsLetter(c)) { // letter = begin identifier
                string ident = "";
                while (i < input.Length && IsLetter(input[i])) {
                    ident += input[i];
                    i++;
                }
                if (ident.Length==0) throw new CompileException("Identifier name is empty/invalid" + "\n At " + lineNumber + ":" + lineIndex);
                tokenList.Add(new IdentifierToken(ident));
            } else if (c == '#') { // # = begin setting
                string ident = "";
                i++;
                while (i < input.Length && IsLetter(input[i])) {
                    ident += input[i];
                    i++;
                }
                if (ident.Length==0) throw new CompileException("Identifier name is empty/invalid" + "\n At " + lineNumber + ":" + lineIndex);
                tokenList.Add(new SettingToken(ident));
            } else if (c == '{') { // # = begin code block
                // TODO code parser
            } else if (c == '"' || c == '\'') { // " = begin string
                char f = c;
                string str = "";
                i++;
                while (i < input.Length && input[i]!=f) {
                    str += input[i];
                    i++;
                }
                tokenList.Add(new StringToken(str));

            } else if (c == ':') {
                tokenList.Add(new ColonToken());
            } else if (c == ',') {
                tokenList.Add(new CommaToken());
            } else if (c == '+') {
                tokenList.Add(new ChoiceDefToken());
            } else if (c == '>') {
                tokenList.Add(new ArrowToken());
            } else if (c == '\n') {
                tokenList.Add(new EndToken());
                lineNumber++;
                lineIndex = 0;
            } else {
                throw new CompileException("Unrecognized token: " + c + "\n At " + lineNumber + ":" + lineIndex);
            }
        }
        return tokenList;
    }

    static bool IsNum(char c) {
        return (c >= '0' && c<= '9');
    }
    static bool IsLetter(char c) {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
}

public class CompileException : System.Exception {
    public CompileException(string msg) : base(msg) {}
}
























/////////////////////////////////////////////////////////////////
// Besoke, evil hack to simulate rust enums. Tread no further. //
/////////////////////////////////////////////////////////////////

public interface Token {}
public interface EmptyToken : Token {}
public class ColonToken : EmptyToken {}
public class EndToken : EmptyToken {}
public class CommaToken : EmptyToken {}
public class ChoiceDefToken : EmptyToken {}
public class ArrowToken : EmptyToken {}

public class DataToken<T> : Token {
    public T data;
    public DataToken(T t) {
        this.data = t;
    }
}
public class SettingToken : DataToken<string> {
    public SettingToken(string s) : base(s) {}
}
public class IdentifierToken : DataToken<string> {
    public IdentifierToken(string s) : base(s) {}
}
public class CodeToken : DataToken<string> {
    public CodeToken(string s) : base(s) {}
}
public class StringToken : DataToken<string> {
    public StringToken(string s) : base(s) {}
}
public class IdentChangeToken : DataToken<int> {
    public IdentChangeToken(int s) : base(s) {}
}