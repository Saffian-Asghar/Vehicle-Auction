"use client";
import React, { useEffect, useState } from "react";
import AuctionCard from "./AuctionCard";
import AppPagination from "../components/AppPagination";
import { Auction, PagedResult } from "@/types";
import { fetchListings } from "../actions/auctionActions";
import Filters from "./Filters";
import { useParamsStore } from "@/hooks/useParamsStore";
import { shallow } from "zustand/shallow";
import qs from "query-string";
import EmptyFilter from "../components/EmptyFilter";

export default function Listings() {
    const [data, setData] = useState<PagedResult<Auction>>();
    const params = useParamsStore(
        (state) => ({
            pageNumber: state.pageNumber,
            pageSize: state.pageSize,
            searchTerm: state.searchTerm,
            orderBy: state.orderBy,
            filterBy: state.filterBy,
        }),
        shallow
    );
    const setParams = useParamsStore((state) => state.setParams);
    const url = qs.stringifyUrl({ url: "", query: params });

    function setPageNumber(pageNumber: number) {
        setParams({ pageNumber });
    }

    useEffect(() => {
        fetchListings(url).then((data) => {
            setData(data);
        });
    }, [url]);

    if (!data) {
        return <div>Loading...</div>;
    }

    return (
        <>
            <Filters />
            {data.totalCount === 0 ? (
                <EmptyFilter showReset />
            ) : (
                <>
                    {" "}
                    <div className="grid grid-cols-4 gap-6">
                        {data.results.map((auction) => (
                            <AuctionCard key={auction.id} auction={auction} />
                        ))}
                    </div>
                    <div className="flex justify-center mt-4">
                        <AppPagination
                            currentPage={params.pageNumber}
                            pageCount={data.pageCount}
                            pageChanged={setPageNumber}
                        />
                    </div>
                </>
            )}
        </>
    );
}
