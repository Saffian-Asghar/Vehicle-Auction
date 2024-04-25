import React from "react";

type Props = {
    amount?: number;
    reservePrice: number;
};
export default function CurrentBid({ amount, reservePrice }: Props) {
    const text = amount ? '$' + amount : 'No bids yet';
    const color = amount ? amount > reservePrice ? 'bg-green-500' : 'bg-amber-500' : 'bg-red-500';
    return (
        <div className={`
            border-2
            border-white
            text-white
            py-1
            px-2
            rounded-lg
            flex
            justify-center
            ${color}
        `}>
            {text}
        </div>
    );
}
