"use client";

import { deleteAuction } from "@/app/actions/auctionActions";
import { Button } from "flowbite-react";
import { useRouter } from "next/navigation";
import React from "react";
import toast from "react-hot-toast";

type Props = {
    id: string;
};
export default function DeleteButton({ id }: Props) {
    const [loading, setLoading] = React.useState(false);
    const router = useRouter();

    function onDelete() {
        setLoading(true);
        deleteAuction(id)
            .then((res) => {
                if (res.error) throw res.error;
                toast.success("Auction deleted successfully");
                router.push("/");
            })
            .catch((error: any) => {
                toast.error(error.status + " " + error.message);
            })
            .finally(() => setLoading(false));
    }
    return (
        <Button color='failure' isProcessing={loading} onClick={onDelete}>
            Delete
        </Button>
    );
}
