namespace PacTheMan.Models {

    public static class PositionExtension {

        //
        // Summary:
        //     Checks wheter a Position "otherPos" is in range of this position "selfPos".
        //
        // Parameters:
        //   otherPos:
        //     The position to check.
        //
        //   range:
        //     Range in which the other position still counts as the same; defaults to 1.
        //
        public static bool IsEqualUpToRange(this BasePosition selfPos, BasePosition otherPos, int range = 1) {
            return (selfPos.X - range <= otherPos.X && selfPos.X + range >= otherPos.X) 
                    && (selfPos.Y - range <= otherPos.Y&& selfPos.Y + range >= otherPos.Y);
        }

    }
}