import { Auction, PagedResult } from "@/types";
import { createWithEqualityFn } from "zustand/traditional";

type State = {
    auctions: Auction[];
    totalCount: number;
    pageCount: number;
};

type Actions = {
    setData: (data: PagedResult<Auction>) => void;
    setCurrentPrice: (auctionId: string, price: number) => void;
}

const initialState: State = {
    auctions: [],
    totalCount: 0,
    pageCount: 0,
};

export const useAuctionStore = createWithEqualityFn<State & Actions>((set) => ({
    ...initialState,
    setData: (data : PagedResult<Auction>) => set((state) => ({
        ...state,
        auctions: data.results,
        totalCount: data.totalCount,
        pageCount: data.pageCount,
    })),
    setCurrentPrice: (auctionId : string, price : number) => set((state) => ({
        ...state,
        auctions: state.auctions.map((auction) => {
            if (auction.id === auctionId) {
                return {
                    ...auction,
                    currentHighBid: price,
                };
            }
            return auction;
        }),
    })),
}));
